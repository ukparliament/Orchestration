using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationConstituencyOS
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //Constituency
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:ConstituencyGroup"));
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupOnsCode", doc, "rdf:RDF/rdf:Description/admingeo:gssCode", settings.SourceXmlNamespaceManager);
            //Constituency Area
            generateConstituencyArea(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);

            return result;
        }

        private void generateConstituencyArea(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
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
            TripleGenerator.GenerateTriple((Graph)graph, constituencyAreaNode, "parl:constituencyAreaLatitude", doc, "rdf:RDF/rdf:Description/wgs84:lat", namespaceManager, "xsd:decimal");
            TripleGenerator.GenerateTriple((Graph)graph, constituencyAreaNode, "parl:constituencyAreaLongitude", doc, "rdf:RDF/rdf:Description/wgs84:long", namespaceManager, "xsd:decimal");
            generateTriplePolygon(graph, constituencyAreaNode, doc, namespaceManager);
            graph.Assert(subject, graph.CreateUriNode("parl:constituencyGroupHasConstituencyArea"), constituencyAreaNode);
        }

        private void generateTriplePolygon(IGraph graph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
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

        private string convertNEtoLongLat(string nePair)
        {
            string[] arr = nePair.Split(',');
            double northing = Convert.ToDouble(arr[1]);
            double easting = Convert.ToDouble(arr[0]);
            return NEtoLatLongConversion.GetLongitudeLatitude(northing, easting);
        }
    }
}
