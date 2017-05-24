using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions.Transformation
{
    public static class TransformationLordsTypeMnis
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static Stopwatch timer = null;
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private static readonly string graphSparql = @"
        construct {
        	?houseIncumbencyType a parl:HouseIncumbencyType;
                parl:houseIncumbencyTypeMnisId ?houseIncumbencyTypeMnisId;
                parl:houseIncumbencyTypeName ?houseIncumbencyTypeName.
        }
        where {
            bind(@houseIncumbencyTypeUri as ?houseIncumbencyType)
        	?houseIncumbencyType a parl:HouseIncumbencyType.
            optional {?houseIncumbencyType parl:houseIncumbencyTypeMnisId ?houseIncumbencyTypeMnisId}
            optional {?houseIncumbencyType parl:houseIncumbencyTypeName ?houseIncumbencyTypeName}
        }";


        [FunctionName("TransformationLordsTypeMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "TransformationLordsTypeMnis";
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();

            telemetryClient.TrackEvent("Triggered");
            timer = Stopwatch.StartNew();
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if ((data.url == null) || (data.callbackUrl == null))
            {
                timer.Stop();
                telemetryClient.TrackTrace("Missing some value(s)", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            telemetryClient.Context.Properties["DataUrl"] = data.url.ToString();
            if (data.batchId != null)
                telemetryClient.Context.Properties["BatchId"] = data.batchId.ToString();
            new Thread(() => startProcess(data.url.ToString(), data.callbackUrl.ToString())).Start();

            return req.CreateResponse();
        }

        private static async Task<HttpResponseMessage> communicateBack(string callbackUrl, string errorMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                timer.Stop();
                if (string.IsNullOrWhiteSpace(errorMessage))
                    telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                else
                    telemetryClient.TrackEvent(errorMessage, new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return await client.PostAsync(callbackUrl, new StringContent(errorMessage ?? "OK"));
            }
        }

        private static XmlNamespaceManager getNamespaceManager()
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            namespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            namespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");

            return namespaceManager;
        }

        private static async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl)
        {
            string fullDataUrl = $"{dataUrl}?$select=LordsMembershipType_Id,Name";
            string xml = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                telemetryClient.TrackTrace("Contacting source", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                xml = await XmlDataRetrieval.GetXmlDataFromUrl(fullDataUrl, "application/atom+xml");
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while retrieving data from {dataUrl}");
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("XmlDataRetrieval", fullDataUrl, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (string.IsNullOrWhiteSpace(xml))
            {
                telemetryClient.TrackTrace("Empty response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                return await communicateBack(callbackUrl, $"Empty response for {dataUrl}");
            }
            XDocument doc = null;
            try
            {
                telemetryClient.TrackTrace("Parsing response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                doc = XDocument.Parse(xml);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while reading data as xml for {dataUrl}");
            }
            XmlNamespaceManager xmlNamespaceManager = getNamespaceManager();
            Uri subjectUri = null;
            try
            {
                telemetryClient.TrackTrace("Getting key", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                XElement key = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:LordsMembershipType_Id", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Key: {key.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                subjectUri = IdRetrieval.GetSubject("HouseIncumbencyType", "houseIncumbencyTypeMnisId", key.Value, true, telemetryClient);
                telemetryClient.TrackTrace($"Subject: {subjectUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem while obtaining key for {dataUrl}");
            }
            IGraph oldGraph = null;
            try
            {
                telemetryClient.TrackTrace("Trying to get existing graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                oldGraph = generateExistingGraph(subjectUri);
                if (oldGraph == null)
                {
                    telemetryClient.TrackTrace($"Cannot retrieve old graph for {subjectUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                    return await communicateBack(callbackUrl, $"Cannot retrieve old graph for {subjectUri}");
                }
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem with retrieving old graph for {subjectUri}");
            }
            IGraph newGraph = null;
            try
            {
                telemetryClient.TrackTrace("Generating new graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                newGraph = generateNewGraph(doc, oldGraph, subjectUri, xmlNamespaceManager);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem with retrieving old graph for {subjectUri}");
            }
            GraphDiffReport difference = oldGraph.Difference(newGraph);
            telemetryClient.TrackTrace($"Found triples: {difference.AddedTriples.Count()} added and {difference.RemovedTriples.Count()} removed", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            //foreach(Triple t in difference.AddedTriples)
            //  log.Info($"{t}");
            //foreach(Triple t in difference.RemovedTriples)
            //  log.Info($"{t}");
            if ((difference.AddedTriples.Any() == false) && (difference.RemovedTriples.Any() == false))
            {
                telemetryClient.TrackTrace("No update required", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                return await communicateBack(callbackUrl);
            }
            externalTimer = Stopwatch.StartNew();
            externalStartTime = DateTime.UtcNow;
            externalCallOk = true;
            try
            {
                telemetryClient.TrackTrace("Updating graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                GraphUpdate.UpdateDifference(difference);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
                return await communicateBack(callbackUrl, $"Problem when updating graph for {subjectUri}");
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("GraphUpdate", $"+{difference.AddedTriples.Count()}/-{difference.RemovedTriples.Count()}", externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return await communicateBack(callbackUrl);
        }

        private static IGraph generateExistingGraph(Uri subject)
        {
            SparqlParameterizedString sparql = new SparqlParameterizedString(graphSparql);
            sparql.SetUri("houseIncumbencyTypeUri", subject);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));

            return GraphRetrieval.GetGraph(sparql.ToString(), telemetryClient);
        }

        private static IGraph generateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, XmlNamespaceManager xmlNamespaceManager)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));
            
            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //HouseIncumbencyType
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:HouseIncumbencyType"));
            TripleGenerator.GenerateTriple(result, subject, "parl:houseIncumbencyTypeMnisId", doc, "atom:entry/atom:content/m:properties/d:LordsMembershipType_Id", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:houseIncumbencyTypeName", doc, "atom:entry/atom:content/m:properties/d:Name", xmlNamespaceManager);
            return result;
        }
    }
}