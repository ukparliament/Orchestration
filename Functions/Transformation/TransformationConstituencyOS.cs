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
    public static class TransformationConstituencyOS
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static Stopwatch timer = null;
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private static readonly string graphSparql = @"
        construct {
        	?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent;
                parl:constituencyAreaLatitude ?constituencyAreaLatitude;
                parl:constituencyAreaLongitude ?constituencyAreaLongitude.
        }
        where {
            bind(@constituencyGroupUri as ?constituencyGroup)
            ?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent;
                parl:constituencyAreaLatitude ?constituencyAreaLatitude;
                parl:constituencyAreaLongitude ?constituencyAreaLongitude.
        }";

        [FunctionName("TransformationConstituencyOS")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "TransformationConstituencyOS";
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
            namespaceManager.AddNamespace("admingeo", "http://data.ordnancesurvey.co.uk/ontology/admingeo/");
            namespaceManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            namespaceManager.AddNamespace("owl", "http://www.w3.org/2002/07/owl#");
            namespaceManager.AddNamespace("geometry", "http://data.ordnancesurvey.co.uk/ontology/geometry/");
            namespaceManager.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
            namespaceManager.AddNamespace("wgs84", "http://www.w3.org/2003/01/geo/wgs84_pos#");

            return namespaceManager;
        }

        private static async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl)
        {
            string osCommand = @"
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
        PREFIX geometry: <http://data.ordnancesurvey.co.uk/ontology/geometry/>
        PREFIX admingeo: <http://data.ordnancesurvey.co.uk/ontology/admingeo/>
        PREFIX wgs84: <http://www.w3.org/2003/01/geo/wgs84_pos#>
        PREFIX owl: <http://www.w3.org/2002/07/owl#>
        
        CONSTRUCT { 
        	@constituency rdfs:label ?label;
        		admingeo:gssCode ?gssCode;
        		wgs84:lat ?lat;
        		wgs84:long ?long;
        		owl:sameAs ?sameAs;
        		geometry:asGML ?gml.
        } WHERE {
        	@constituency rdfs:label ?label;
        		admingeo:gssCode ?gssCode;
        		wgs84:lat ?lat;
        		wgs84:long ?long;
        		owl:sameAs ?sameAs;
        		geometry:extent ?extent.
        	?extent geometry:asGML ?gml;
        }";
            SparqlParameterizedString osSparql = new SparqlParameterizedString(osCommand);
            osSparql.SetUri("constituency", new Uri(dataUrl));
            string fullDataUrl = $"http://data.ordnancesurvey.co.uk/datasets/os-linked-data/apis/sparql?query={WebUtility.UrlEncode(osSparql.ToString())}";
            string xml = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                telemetryClient.TrackTrace("Contacting source", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                xml = await XmlDataRetrieval.GetXmlDataFromUrl(fullDataUrl, "application/rdf+xml");
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
                XElement key = doc.XPathSelectElement("rdf:RDF/rdf:Description/admingeo:gssCode", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Key: {key.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                subjectUri = IdRetrieval.GetSubject("ConstituencyGroup", "constituencyGroupOnsCode", key.Value, true, telemetryClient);
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
            sparql.SetUri("constituencyGroupUri", subject);
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
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupOnsCode", doc, "rdf:RDF/rdf:Description/admingeo:gssCode", xmlNamespaceManager);
            //Constituency Area
            generateConstituencyArea(result, oldGraph, subject, doc, xmlNamespaceManager);

            return result;
        }

        private static void generateConstituencyArea(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Uri constituencyAreaUri;
            IUriNode constituencyAreaTypeNode = graph.CreateUriNode("parl:ConstituencyArea");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            telemetryClient.TrackTrace("Check constituency area", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingConstituencyArea = oldGraph.GetTriplesWithPredicateObject(rdfTypeNode, constituencyAreaTypeNode);
            if ((existingConstituencyArea != null) && (existingConstituencyArea.Any()))
            {
                constituencyAreaUri = ((IUriNode)existingConstituencyArea.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found constituency area {constituencyAreaUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string constituencyAreaId = new IdGenerator.IdMaker().MakeId();
                constituencyAreaUri = new Uri(constituencyAreaId);
                telemetryClient.TrackTrace($"New constituency area {constituencyAreaUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode constituencyAreaNode = graph.CreateUriNode(constituencyAreaUri);

            graph.Assert(constituencyAreaNode, rdfTypeNode, constituencyAreaTypeNode);
            TripleGenerator.GenerateTriple((Graph)graph, constituencyAreaNode, "parl:constituencyAreaLatitude", doc, "rdf:RDF/rdf:Description/wgs84:lat", xmlNamespaceManager, "xsd:decimal");
            TripleGenerator.GenerateTriple((Graph)graph, constituencyAreaNode, "parl:constituencyAreaLongitude", doc, "rdf:RDF/rdf:Description/wgs84:long", xmlNamespaceManager, "xsd:decimal");
            generateTriplePolygon(graph, constituencyAreaNode, doc, xmlNamespaceManager);
            graph.Assert(subject, graph.CreateUriNode("parl:constituencyGroupHasConstituencyArea"), constituencyAreaNode);
        }

        private static void generateTriplePolygon(IGraph graph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
        {
            Uri dataTypeUri = new Uri("http://www.opengis.net/ont/geosparql#wktLiteral");
            XElement element = doc.XPathSelectElement("rdf:RDF/rdf:Description/geometry:asGML", namespaceManager);
            string xmlPolygon = element.Value.ToString().Replace("gml:", string.Empty);
            foreach (XElement polygon in XDocument.Parse(xmlPolygon).Descendants("coordinates"))
            {
                string longLat = string.Join(",", polygon.Value.ToString().Split(' ').Select(ne => convertNEtoLongLat(ne)));
                INode objectNode = graph.CreateLiteralNode($"Polygon(({longLat}))", dataTypeUri);
                graph.Assert(subject, graph.CreateUriNode("parl:constituencyAreaExtent"), objectNode);
            }
        }

        private static string convertNEtoLongLat(string nePair)
        {
            string[] arr = nePair.Split(',');
            double northing = Convert.ToDouble(arr[1]);
            double easting = Convert.ToDouble(arr[0]);
            return NEtoLatLongConversion.GetLongitudeLatitude(northing, easting);
        }
    }
}