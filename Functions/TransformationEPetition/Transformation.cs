using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions.TransformationEPetition
{
    public class Transformation :BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //EPetition
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:EPetition"));
            TripleGenerator.GenerateTriple(result, subject, "parl:ePetitionUkgapId", doc, "root/data/id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:openedAt", doc, "root/data/attributes/open_at", settings.SourceXmlNamespaceManager, "xsd:dateTime");
            TripleGenerator.GenerateTriple(result, subject, "parl:background", doc, "root/data/attributes/background", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:additionalDetails", doc, "root/data/attributes/additional_details", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:closedAt", doc, "root/data/attributes/closed_at", settings.SourceXmlNamespaceManager, "xsd:dateTime");
            TripleGenerator.GenerateTriple(result, subject, "parl:action", doc, "root/data/attributes/action", settings.SourceXmlNamespaceManager);
            generateTripleGovernmentResponse(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            generateTripleDebate(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            generateTripleLocatedSignatureCount(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            
            return result;
        }

        private void generateTripleGovernmentResponse(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            IUriNode governmentResponseNode = graph.CreateUriNode("parl:ePetitionHasGovernmentResponse");
            IUriNode governmentResponse = generateLinkedObject(graph, oldGraph, subject, governmentResponseNode);
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseCreatedAt", doc, "root/data/attributes/government_response/created_at", xmlNamespaceManager, "xsd:dateTime");
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseUpdatedAt", doc, "root/data/attributes/government_response/updated_at", xmlNamespaceManager, "xsd:dateTime");
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseDetails", doc, "root/data/attributes/government_response/details", xmlNamespaceManager);
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseSummary", doc, "root/data/attributes/government_response/summary", xmlNamespaceManager);
        }

        private void generateTripleDebate(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            IUriNode debateNode = graph.CreateUriNode("parl:ePetitionHasDebate");
            IUriNode debate = generateLinkedObject(graph, oldGraph, subject, debateNode);
            TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateProposedDate", doc, "root/data/attributes/scheduled_debate_date", xmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateDate", doc, "root/data/attributes/debate/debated_on", xmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateVideoUrl", doc, "root/data/attributes/debate/video_url", xmlNamespaceManager);
            TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateTranscriptUrl", doc, "root/data/attributes/debate/transcript_url", xmlNamespaceManager);
            TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateOverview", doc, "root/data/attributes/debate/overview", xmlNamespaceManager);
        }
        private void generateTripleThresholdAttainment(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Dictionary<string, string> thresholds = IdRetrieval.GetSubjects("Threshold", "thresholdName", telemetryClient);
            foreach (KeyValuePair<string, string> threshold in thresholds)
            {
                IUriNode thresholdAttainment = generateLinkedSubject(graph, oldGraph, graph.CreateUriNode("parl:thresholdAttainmentHasThreshold"), graph.CreateUriNode(threshold.Value));
                string thresholdPath = null;
                switch (threshold.Key.ToLower())
                {
                    case "moderation":
                        thresholdPath = "moderation_threshold_reached_at";
                        break;
                    case "response":
                        thresholdPath = "response_threshold_reached_at";
                        break;
                    case "debate":
                        thresholdPath = "debate_threshold_reached_at";
                        break;
                }
                TripleGenerator.GenerateTriple((Graph)graph, thresholdAttainment, "parl:thresholdAttainmentAt", doc, $"root/data/attributes/{thresholdPath}", xmlNamespaceManager, "xsd:dateTime");
                graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasThresholdAttainment"), thresholdAttainment);
                graph.Assert(thresholdAttainment, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:ThresholdAttainment"));
            }
        }
        private void generateTripleLocatedSignatureCount(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Dictionary<string, string> constituencies = IdRetrieval.GetSubjects("ConstituencyGroup", "constituencyGroupOnsCode", telemetryClient);

            foreach (XElement constituency in doc.XPathSelectElements("root/data/attributes/signatures_by_constituency"))
            {
                XElement onsCode = constituency.Element("ons_code");
                if ((onsCode != null) && (string.IsNullOrWhiteSpace(onsCode.Value)==false) && (constituencies.ContainsKey(onsCode.Value)))
                {
                        XDocument constituencyDoc = new XDocument(constituency);
                        string constituencySignatureCountId = new IdGenerator.IdMaker().MakeId();
                        IUriNode constituencySignatureCount = graph.CreateUriNode(new Uri(constituencySignatureCountId));
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:LocatedSignatureCount"));
                        TripleGenerator.GenerateTriple((Graph)graph, constituencySignatureCount, "parl:signatureCount", constituencyDoc, "signatures_by_constituency/signature_count", xmlNamespaceManager, "xsd:integer");
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode("parl:signatureCountRetrievedAt"), graph.CreateLiteralNode(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), graph.CreateUriNode("xsd:dateTime").Uri));
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode("parl:locatedSignatureCountHasPlace"), graph.CreateUriNode(new Uri(constituencies[onsCode.Value])));
                        graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasLocatedSignatureCount"), constituencySignatureCount);
                }
            }
            Dictionary<string, string> countries = IdRetrieval.GetSubjects("Country", "countryGovRegisterId", telemetryClient);
            Dictionary<string, string> territories = IdRetrieval.GetSubjects("Territory", "territoryGovRegisterId", telemetryClient);
            Dictionary<string, string> internationalAreas = countries.Concat(territories).ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (XElement internationalArea in doc.XPathSelectElements("root/data/attributes/signatures_by_country"))
            {
                XElement internationalAreaCode = internationalArea.Element("code");
                if ((internationalAreaCode != null) && (string.IsNullOrWhiteSpace(internationalAreaCode.Value) == false) && (internationalAreas.ContainsKey(internationalAreaCode.Value)))
                {
                        XDocument countryDoc = new XDocument(internationalArea);
                        string countrySignatureCountId = new IdGenerator.IdMaker().MakeId();
                        IUriNode countrySignatureCount = graph.CreateUriNode(new Uri(countrySignatureCountId));
                        graph.Assert(countrySignatureCount, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:LocatedSignatureCount"));
                        TripleGenerator.GenerateTriple((Graph)graph, countrySignatureCount, "parl:signatureCount", countryDoc, "signatures_by_country/signature_count", xmlNamespaceManager, "xsd:integer");
                        graph.Assert(countrySignatureCount, graph.CreateUriNode("parl:signatureCountRetrievedAt"), graph.CreateLiteralNode(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), graph.CreateUriNode("xsd:dateTime").Uri));
                        graph.Assert(countrySignatureCount, graph.CreateUriNode("parl:locatedSignatureCountHasPlace"), graph.CreateUriNode(new Uri(internationalAreas[internationalAreaCode.Value])));
                        graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasLocatedSignatureCount"), countrySignatureCount);
                }
            }

        }

        private void generateTripleModeration(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Uri governmentResponseUri;
            IUriNode governmentResponseNode = graph.CreateUriNode("parl:ePetitionHasGovernmentResponse");
            IEnumerable<Triple> existingGovernmentResponses = oldGraph.GetTriplesWithPredicate(governmentResponseNode);
            if ((existingGovernmentResponses != null) && (existingGovernmentResponses.Any()))
            {
                governmentResponseUri = ((IUriNode)existingGovernmentResponses.SingleOrDefault().Object).Uri;
            }
            else
            {
                string governmentResponseId = new IdGenerator.IdMaker().MakeId();
                governmentResponseUri = new Uri(governmentResponseId);
                telemetryClient.TrackTrace($"New government response {governmentResponseUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode governmentResponse = graph.CreateUriNode(governmentResponseUri);
            graph.Assert(subject, governmentResponseNode, governmentResponse);
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseCreatedAt", doc, "root/data/attributes/government_response/created_at", xmlNamespaceManager, "xsd:dateTime");
        }

        private IUriNode generateLinkedObject(IGraph graph, IGraph oldGraph, IUriNode subject, IUriNode linkedPredicateNode)
        {
            Uri linkedPredicateIdUri;
            IEnumerable<Triple> existingLinkedTriples = null;
            existingLinkedTriples = oldGraph.GetTriplesWithPredicate(linkedPredicateNode);

            if ((existingLinkedTriples != null) && (existingLinkedTriples.Any()))
            {
                linkedPredicateIdUri = ((IUriNode)existingLinkedTriples.SingleOrDefault().Object).Uri;
            }
            else
            {
                string linkedPredicateId = new IdGenerator.IdMaker().MakeId();
                linkedPredicateIdUri = new Uri(linkedPredicateId);
                telemetryClient.TrackTrace($"New {linkedPredicateNode.Uri} {linkedPredicateIdUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode linkedPredicateIdNode = graph.CreateUriNode(linkedPredicateIdUri);
            graph.Assert(subject, linkedPredicateNode, linkedPredicateIdNode);
            return linkedPredicateIdNode;
        }
        private IUriNode generateLinkedSubject(IGraph graph, IGraph oldGraph, IUriNode linkedPredicateNode, INode objectNode)
        {
            Uri linkedPredicateIdUri;
            IEnumerable<Triple> existingLinkedTriples = null;
            existingLinkedTriples = oldGraph.GetTriplesWithPredicateObject(linkedPredicateNode, objectNode);
            if ((existingLinkedTriples != null) && (existingLinkedTriples.Any()))
            {
                linkedPredicateIdUri = ((IUriNode)existingLinkedTriples.SingleOrDefault().Subject).Uri;
            }
            else
            {
                string linkedPredicateId = new IdGenerator.IdMaker().MakeId();
                linkedPredicateIdUri = new Uri(linkedPredicateId);
                telemetryClient.TrackTrace($"New {linkedPredicateNode.Uri} {linkedPredicateIdUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode linkedPredicateIdNode = graph.CreateUriNode(linkedPredicateIdUri);
            graph.Assert(linkedPredicateIdNode, linkedPredicateNode, objectNode);
            return linkedPredicateIdNode;
        }
    }
}
