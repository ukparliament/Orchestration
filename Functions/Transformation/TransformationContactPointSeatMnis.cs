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
    public static class TransformationContactPointSeatMnis
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static Stopwatch timer = null;
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private static readonly string graphSparql = @"
        construct {
        	?contactPoint a parl:ContactPoint;
                parl:contactPointMnisId ?contactPointMnisId;
                parl:email ?email;
                parl:faxNumber ?faxNumber;
                parl:phoneNumber ?phoneNumber;
                parl:contactPointHasPostalAddress ?postalAddress.
            ?postalAddress a parl:PostalAddress;
                parl:addressLine1 ?addressLine1;
                parl:addressLine2 ?addressLine2;
                parl:addressLine3 ?addressLine3;
                parl:addressLine4 ?addressLine4;
                parl:addressLine5 ?addressLine5;
                parl:postCode ?postCode.
            ?incumbency parl:incumbencyHasContactPoint ?contactPoint.
        }
        where {
            bind(@contactPointUri as ?contactPoint)
            ?contactPoint a parl:ContactPoint;
                parl:contactPointMnisId ?contactPointMnisId.
            optional {?contactPoint parl:email ?email}
            optional {?contactPoint parl:faxNumber ?faxNumber}
            optional {?contactPoint parl:phoneNumber ?phoneNumber}
            optional {
                ?contactPoint parl:contactPointHasPostalAddress ?postalAddress.
                optional {?postalAddress parl:addressLine1 ?addressLine1}
                optional {?postalAddress parl:addressLine2 ?addressLine2}
                optional {?postalAddress parl:addressLine3 ?addressLine3}
                optional {?postalAddress parl:addressLine4 ?addressLine4}
                optional {?postalAddress parl:addressLine5 ?addressLine5}
                optional {?postalAddress parl:postCode ?postCode}
            }
            optional {?incumbency parl:incumbencyHasContactPoint ?contactPoint}
        }";

        [FunctionName("TransformationContactPointSeatMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "TransformationContactPointSeatMnis";
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
            string fullDataUrl = $"{dataUrl}?$select=MemberAddress_Id,Member_Id,Address1,Address2,Address3,Address4,Address5,Postcode,Email,Phone,Fax";
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
                XElement key = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:MemberAddress_Id", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Key: {key.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                subjectUri = IdRetrieval.GetSubject("ContactPoint", "contactPointMnisId", key.Value, true, telemetryClient);
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
                return await communicateBack(callbackUrl, $"Problem with creating new graph for {subjectUri}");
            }
            GraphDiffReport difference = oldGraph.Difference(newGraph);
            telemetryClient.TrackTrace($"Found triples: {difference.AddedTriples.Count()} added and {difference.RemovedTriples.Count()} removed", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            //foreach(Triple t in difference.AddedTriples)
            //log.Info($"{t}");
            //foreach(Triple t in difference.RemovedTriples)
            //log.Info($"{t}");
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
            sparql.SetUri("contactPointUri", subject);
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
            //Contact point
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:ContactPoint"));
            TripleGenerator.GenerateTriple(result, subject, "parl:contactPointMnisId", doc, "atom:entry/atom:content/m:properties/d:MemberAddress_Id", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:email", doc, "atom:entry/atom:content/m:properties/d:Email", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:faxNumber", doc, "atom:entry/atom:content/m:properties/d:Fax", xmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:phoneNumber", doc, "atom:entry/atom:content/m:properties/d:Phone", xmlNamespaceManager);
            //Postal Address
            generateAddressTriples(result, oldGraph, subject, doc, xmlNamespaceManager);
            //hasContact
            generateTripleHasContact(result, oldGraph, subject, doc, xmlNamespaceManager);

            return result;
        }

        private static void generateAddressTriples(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
        {
            XElement address;
            bool isFound = false;
            telemetryClient.TrackTrace("Generating address", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode postalAddressNode = generateTriplePostalAddress(graph, oldGraph, subject);
            int j = 1;
            for (int i = 1; i <= 5; i++)
            {
                address = doc.XPathSelectElement($"atom:entry/atom:content/m:properties/d:Address{i}", namespaceManager);
                if ((address != null) && (string.IsNullOrWhiteSpace(address.Value) == false))
                {
                    graph.Assert(postalAddressNode, graph.CreateUriNode($"parl:addressLine{j}"), graph.CreateLiteralNode(address.Value));
                    isFound = true;
                    j++;
                }
            }
            address = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Postcode", namespaceManager);
            if ((address != null) && (string.IsNullOrWhiteSpace(address.Value) == false))
            {
                graph.Assert(postalAddressNode, graph.CreateUriNode("parl:postCode"), graph.CreateLiteralNode(address.Value));
                isFound = true;
            }
            if (isFound == true)
            {
                graph.Assert(postalAddressNode, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:PostalAddress"));
                graph.Assert(subject, graph.CreateUriNode("parl:contactPointHasPostalAddress"), postalAddressNode);
            }
        }

        private static IUriNode generateTriplePostalAddress(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri postalAddressUri;

            telemetryClient.TrackTrace("Check postal address", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingPostalAddress = oldGraph.GetTriplesWithPredicateObject(graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:PostalAddress"));
            if ((existingPostalAddress != null) && (existingPostalAddress.Any()))
            {
                postalAddressUri = ((IUriNode)existingPostalAddress.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found postal address {postalAddressUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string postalAddressId = new IdGenerator.IdMaker().MakeId();
                postalAddressUri = new Uri(postalAddressId);
                telemetryClient.TrackTrace($"New postal address {postalAddressUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode postalAddressNode = graph.CreateUriNode(postalAddressUri);

            return postalAddressNode;
        }

        private static void generateTripleHasContact(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            string hasContactCommand = @"
        construct {
            ?id a parl:Incumbency.
        }
        where {
            ?id a parl:Incumbency;
                parl:incumbencyHasMember ?incumbencyHasMember;
                parl:incumbencyStartDate ?incumbencyStartDate.
            ?incumbencyHasMember parl:personMnisId @personMnisId.
        } order by desc(?incumbencyStartDate) limit 1";
            Uri hasContactUri;
            IUriNode hasContactPredicateNode = graph.CreateUriNode("parl:incumbencyHasContactPoint");

            telemetryClient.TrackTrace("Check contact", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingContacts = oldGraph.GetTriplesWithPredicate(hasContactPredicateNode);
            if ((existingContacts != null) && (existingContacts.Any()))
            {
                foreach (Triple t in existingContacts)
                {
                    telemetryClient.TrackTrace($"Found contact ({t.Subject})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    graph.Assert(t.Subject, hasContactPredicateNode, subject);
                }
            }
            else
            {
                XElement mnisId = doc.XPathSelectElement($"atom:entry/atom:content/m:properties/d:Member_Id", xmlNamespaceManager);
                if ((mnisId == null) || (string.IsNullOrWhiteSpace(mnisId.Value) == true))
                {
                    throw new Exception($"{subject.Uri}: No member info found");
                }
                SparqlParameterizedString hasContactSparql = new SparqlParameterizedString(hasContactCommand);
                hasContactSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                hasContactSparql.SetLiteral("personMnisId", mnisId.Value);
                hasContactUri = IdRetrieval.GetSubject(hasContactSparql.ToString(), false, telemetryClient);
                if (hasContactUri != null)
                {
                    graph.Assert(graph.CreateUriNode(hasContactUri), hasContactPredicateNode, subject);
                }
                else
                    telemetryClient.TrackTrace("No contact found", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
        }
    }
}