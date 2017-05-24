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
    public static class TransformationConstituencyMnis
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static Stopwatch timer = null;
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private static readonly string houseName = "House of Commons";
        private static readonly string graphSparql = @"
        construct {
        	?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupMnisId ?constituencyGroupMnisId;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupName ?constituencyGroupName;
                parl:constituencyGroupStartDate ?constituencyGroupStartDate;
                parl:constituencyGroupEndDate ?constituencyGroupEndDate.
            ?seat a parl:HouseSeat;
                parl:houseSeatHasHouse ?house;
                parl:houseSeatHasConstituencyGroup ?constituencyGroup.
        }
        where {
            bind(@constituencyGroupUri as ?constituencyGroup)
            ?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupMnisId ?constituencyGroupMnisId.
            optional { ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.}
            optional {?constituencyGroup parl:constituencyGroupName ?constituencyGroupName}
            optional {?constituencyGroup parl:constituencyGroupStartDate ?constituencyGroupStartDate}
            optional {?constituencyGroup parl:constituencyGroupEndDate ?constituencyGroupEndDate}
            ?seat a parl:HouseSeat;
                parl:houseSeatHasHouse ?house;
                parl:houseSeatHasConstituencyGroup ?constituencyGroup.
            ?house parl:houseName @houseName.
        }";

        [FunctionName("TransformationConstituencyMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "TransformationConstituencyMnis";
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
            string fullDataUrl = $"{dataUrl}?$select=Constituency_Id,Name,StartDate,EndDate,ONSCode";
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
                return await communicateBack(callbackUrl, $"Problem while retrieving data for {dataUrl}");
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
                string constituencySparqlCommand = @"
            construct {
                ?constituencyGroup a parl:ConstituencyGroup.
            }
            where {
                {
                    ?constituencyGroup parl:constituencyGroupMnisId ?constituencyGroupMnisId.
                    filter(?constituencyGroupMnisId=@constituencyGroupMnisId)
                }
                union
                {
                    ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.
                    filter(?constituencyGroupOnsCode=@constituencyGroupOnsCode)
                }
            }
        ";
                XElement keyMnis = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Constituency_Id", xmlNamespaceManager);
                XElement keyOns = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:ONSCode", xmlNamespaceManager);
                telemetryClient.TrackTrace($"MNIS key: {keyMnis.Value}, ONS key: {keyOns.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                SparqlParameterizedString sparql = new SparqlParameterizedString(constituencySparqlCommand);
                sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                sparql.SetLiteral("constituencyGroupMnisId", keyMnis.Value);
                sparql.SetLiteral("constituencyGroupOnsCode", keyOns.Value);
                subjectUri = IdRetrieval.GetSubject(sparql.ToString(), true, telemetryClient);
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
            //log.Info($"{t}");
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
            sparql.SetUri("constituencyGroupUri", subject);
            sparql.SetLiteral("houseName", houseName);
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
            //Constituency
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:ConstituencyGroup"));
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupMnisId", doc, "atom:entry/atom:content/m:properties/d:Constituency_Id", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupName", doc, "atom:entry/atom:content/m:properties/d:Name", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupOnsCode", doc, "atom:entry/atom:content/m:properties/d:ONSCode", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupStartDate", doc, "atom:entry/atom:content/m:properties/d:StartDate", xmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupEndDate", doc, "atom:entry/atom:content/m:properties/d:EndDate", xmlNamespaceManager, "xsd:date");
            //House Seat
            generateTripleSeat(result, oldGraph, subject);

            return result;
        }

        private static void generateTripleSeat(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri seatUri;
            IUriNode seatTypeNode = graph.CreateUriNode("parl:HouseSeat");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            telemetryClient.TrackTrace("Check seat", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingSeat = oldGraph.GetTriplesWithPredicateObject(rdfTypeNode, seatTypeNode);
            if ((existingSeat != null) && (existingSeat.Any()))
            {
                seatUri = ((IUriNode)existingSeat.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found seat ({seatUri})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string seatId = new IdGenerator.IdMaker().MakeId();
                seatUri = new Uri(seatId);
                telemetryClient.TrackTrace($"New seat ({seatUri})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode seatNode = graph.CreateUriNode(seatUri);

            graph.Assert(seatNode, rdfTypeNode, seatTypeNode);
            graph.Assert(seatNode, graph.CreateUriNode("parl:houseSeatHasConstituencyGroup"), subject);
            /*IEnumerable<Triple> createdEndDate = graph.GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode("parl:constituencyGroupEndDate"));
            if ((createdEndDate != null) && (createdEndDate.Any()))
            {
                graph.Assert(seatNode, graph.CreateUriNode("parl:houseSeatEndDate"), createdEndDate.SingleOrDefault().Object);
                log.Info($"{subject.Uri}: Past house ({seatNode.Uri})");
            }*/
            generateTripleHouse(graph, oldGraph, seatNode);
        }

        private static void generateTripleHouse(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri houseUri;
            IUriNode housePredicateNode = graph.CreateUriNode("parl:houseSeatHasHouse");
            IEnumerable<Triple> existingHouse = oldGraph.GetTriplesWithSubjectPredicate(subject, housePredicateNode);
            if ((existingHouse != null) && (existingHouse.Any()))
            {
                houseUri = ((IUriNode)existingHouse.SingleOrDefault().Object).Uri;
                telemetryClient.TrackTrace($"Found house ({houseUri})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                houseUri = IdRetrieval.GetSubject("House", "houseName", houseName, false, telemetryClient);
                if (houseUri == null)
                {
                    throw new Exception($"Could not found house ({houseName})");
                }
            }
            graph.Assert(subject, housePredicateNode, graph.CreateUriNode(houseUri));
        }

    }
}