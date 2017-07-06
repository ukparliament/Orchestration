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
    public class Transformation : BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            logger.Verbose("Generate triples");
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
            TripleGenerator.GenerateTriple(result, subject, "parl:updatedAt", doc, "root/data/attributes/updated_at", settings.SourceXmlNamespaceManager, "xsd:dateTime");
            generateTripleGovernmentResponse(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            generateTripleDebate(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            generateTripleLocatedSignatureCount(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            generateTripleModeration(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);

            return result;
        }

        private void generateTripleGovernmentResponse(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            XElement governmentResponseElement = doc.XPathSelectElement("root/data/attributes/government_response");
            if (governmentResponseElement != null && governmentResponseElement.IsEmpty == false)
            {
                IUriNode governmentResponseNode = graph.CreateUriNode("parl:ePetitionHasGovernmentResponse");
                IUriNode governmentResponse = generateLinkedObject(graph, oldGraph, subject, governmentResponseNode);
                graph.Assert(governmentResponse, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:GovernmentResponse"));
                TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseCreatedAt", doc, "root/data/attributes/government_response/created_at", xmlNamespaceManager, "xsd:dateTime");
                TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseUpdatedAt", doc, "root/data/attributes/government_response/updated_at", xmlNamespaceManager, "xsd:dateTime");
                TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseDetails", doc, "root/data/attributes/government_response/details", xmlNamespaceManager);
                TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseSummary", doc, "root/data/attributes/government_response/summary", xmlNamespaceManager);
            }
        }

        private void generateTripleDebate(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            XElement debateElement = doc.XPathSelectElement("root/data/attributes/debate");
            if (debateElement != null && debateElement.IsEmpty == false)
            {
                IUriNode debateNode = graph.CreateUriNode("parl:ePetitionHasDebate");
                IUriNode debate = generateLinkedObject(graph, oldGraph, subject, debateNode);
                graph.Assert(debate, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:Debate"));
                TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateProposedDate", doc, "root/data/attributes/scheduled_debate_date", xmlNamespaceManager, "xsd:date");
                TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateDate", doc, "root/data/attributes/debate/debated_on", xmlNamespaceManager, "xsd:date");
                TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateVideoUrl", doc, "root/data/attributes/debate/video_url", xmlNamespaceManager);
                TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateTranscriptUrl", doc, "root/data/attributes/debate/transcript_url", xmlNamespaceManager);
                TripleGenerator.GenerateTriple((Graph)graph, debate, "parl:debateOverview", doc, "root/data/attributes/debate/overview", xmlNamespaceManager);
            }
        }
        private void generateTripleThresholdAttainment(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Dictionary<string, string> thresholds = IdRetrieval.GetSubjects("Threshold", "thresholdName", logger);
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
            Dictionary<string, string> constituencies = IdRetrieval.GetSubjects("ConstituencyGroup", "constituencyGroupOnsCode", logger);

            foreach (XElement constituency in doc.XPathSelectElements("root/data/attributes/signatures_by_constituency"))
            {
                XElement onsCode = constituency.Element("ons_code");
                if ((onsCode != null) && (string.IsNullOrWhiteSpace(onsCode.Value) == false) && (constituencies.ContainsKey(onsCode.Value)))
                {
                    XDocument constituencyDoc = new XDocument(constituency);

                    var count = constituency.Element("signature_count").Value;
                    var onsCodeValue = onsCode.Value;
                    var locatedSignatureCountForOnsCode = oldGraph.GetTriplesWithPredicateObject(graph.CreateUriNode("parl:locatedSignatureCountHasPlace"), graph.CreateUriNode(new Uri(constituencies[onsCode.Value])));
                    var runIt = true;

                    if (locatedSignatureCountForOnsCode!=null && locatedSignatureCountForOnsCode.Any())
                    {
                        var oldCountTriple = oldGraph.GetTriplesWithSubjectPredicate(locatedSignatureCountForOnsCode.SingleOrDefault().Subject, graph.CreateUriNode("parl:signatureCount")).SingleOrDefault();
                        var oldCountNode = oldCountTriple.Object;

                        var oldCount = (oldCountNode as ILiteralNode).Value;

                        if (oldCount == count)
                        {
                            runIt = false;
                            foreach (Triple triple in oldGraph.GetTriplesWithSubject(oldCountTriple.Subject))
                            {
                                graph.Assert(triple);
                            }
                            graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasLocatedSignatureCount"), oldCountTriple.Subject);

                        }
                    }

                    if (runIt)
                    {
                        string constituencySignatureCountId = new IdGenerator.IdMaker().MakeId();
                        IUriNode constituencySignatureCount = graph.CreateUriNode(new Uri(constituencySignatureCountId));
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:LocatedSignatureCount"));
                        TripleGenerator.GenerateTriple((Graph)graph, constituencySignatureCount, "parl:signatureCount", constituencyDoc, "signatures_by_constituency/signature_count", xmlNamespaceManager, "xsd:integer");
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode("parl:signatureCountRetrievedAt"), graph.CreateLiteralNode(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), graph.CreateUriNode("xsd:dateTime").Uri));
                        graph.Assert(constituencySignatureCount, graph.CreateUriNode("parl:locatedSignatureCountHasPlace"), graph.CreateUriNode(new Uri(constituencies[onsCode.Value])));
                        graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasLocatedSignatureCount"), constituencySignatureCount);
                    }
                }
            }
            Dictionary<string, string> countries = IdRetrieval.GetSubjects("Country", "countryGovRegisterId", logger);
            Dictionary<string, string> territories = IdRetrieval.GetSubjects("Territory", "territoryGovRegisterId", logger);
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
            XElement approvedAt = doc.XPathSelectElement("root/data/attributes/open_at");
            if ((approvedAt != null) && (string.IsNullOrWhiteSpace(approvedAt.Value) == false))
            {
                IUriNode approval = generateLinkedSubject(graph, oldGraph, graph.CreateUriNode("parl:approvedAt"), graph.CreateLiteralNode(approvedAt.Value, graph.CreateUriNode("xsd:dateTime").Uri));
                graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasModeration"), approval);
                graph.Assert(approval, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:Moderation"));
            }
            XElement rejectedAt = doc.XPathSelectElement("root/data/attributes/rejected_at");
            if ((rejectedAt != null) && (string.IsNullOrWhiteSpace(rejectedAt.Value) == false))
            {
                XElement rejectionCode = doc.XPathSelectElement("root/data/attributes/rejection/code");
                IUriNode rejection = generateLinkedSubject(graph, oldGraph, graph.CreateUriNode("parl:rejectedAt"), graph.CreateLiteralNode(rejectedAt.Value, graph.CreateUriNode("xsd:dateTime").Uri));
                graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasModeration"), rejection);
                graph.Assert(rejection, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:Moderation"));
                if ((rejectionCode != null) && (string.IsNullOrWhiteSpace(rejectionCode.Value) == false))
                {
                    Uri rejectionCodeUri = IdRetrieval.GetSubject("RejectionCode", "rejectionCodeName", rejectionCode.Value, false, logger);
                    if (rejectionCodeUri != null)
                    {
                        graph.Assert(rejection, graph.CreateUriNode("parl:rejectionHasRejectionCode"), graph.CreateUriNode(rejectionCodeUri));
                    }
                    else
                    {
                        logger.Warning($"Found rejected petition with a rejection code not currently supported - {rejectionCode.Value}");
                    }
                }
                else
                {
                    logger.Warning("Found rejected petition with no rejection code");
                }
                TripleGenerator.GenerateTriple((Graph)graph, rejection, "parl:rejectionDetails", doc, "root/data/attributes/rejection/details", xmlNamespaceManager);
            }

            IEnumerable<Triple> moderations = oldGraph.GetTriplesWithObject(graph.CreateUriNode("parl:Moderation"));
            foreach (Triple moderation in moderations)
            {
                graph.Assert(moderation);
                graph.Assert(subject, graph.CreateUriNode("parl:ePetitionHasModeration"), moderation.Subject);
                foreach (Triple triple in oldGraph.GetTriplesWithSubject(moderation.Subject))
                {
                    graph.Assert(triple);
                }
            }
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
                logger.Verbose($"New {linkedPredicateNode.Uri} {linkedPredicateIdUri}");
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
                logger.Verbose($"New {linkedPredicateNode.Uri} {linkedPredicateIdUri}");
            }
            IUriNode linkedPredicateIdNode = graph.CreateUriNode(linkedPredicateIdUri);
            graph.Assert(linkedPredicateIdNode, linkedPredicateNode, objectNode);
            return linkedPredicateIdNode;
        }
    }
}