using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationConstituencyOSNI
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
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupOnsCode", doc, "root/features/attributes/PC_ID", null);
            //Constituency Area
            generateConstituencyArea(result, oldGraph, subject, doc);

            return result;
        }

        private void generateConstituencyArea(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc)
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
            generateTriplePolygon(graph, constituencyAreaNode, doc);
            graph.Assert(subject, graph.CreateUriNode("parl:constituencyGroupHasConstituencyArea"), constituencyAreaNode);
        }

        private void generateTriplePolygon(IGraph graph, IUriNode subject, XDocument doc)
        {
            Uri dataTypeUri = new Uri("http://www.opengis.net/ont/geosparql#wktLiteral");
            foreach (XElement polygon in doc.XPathSelectElements("root/features/geometry/rings"))
            {
                string longLat = string.Join(",", polygon.Elements("rings").Select(p => string.Join(" ", p.Elements("rings").Select(e => e.Value))));
                INode objectNode = graph.CreateLiteralNode($"Polygon(({longLat}))", dataTypeUri);
                graph.Assert(subject, graph.CreateUriNode("parl:constituencyAreaExtent"), objectNode);
            }
        }
    }
}
