using Functions.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationConstituencyMnis
{
    public class Transformation: BaseTransformation<Settings>
    {

        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            logger.Verbose("Generate triples");
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //Constituency
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:ConstituencyGroup"));
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupMnisId", doc, "atom:entry/atom:content/m:properties/d:Constituency_Id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupName", doc, "atom:entry/atom:content/m:properties/d:Name", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupOnsCode", doc, "atom:entry/atom:content/m:properties/d:ONSCode", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupStartDate", doc, "atom:entry/atom:content/m:properties/d:StartDate", settings.SourceXmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:constituencyGroupEndDate", doc, "atom:entry/atom:content/m:properties/d:EndDate", settings.SourceXmlNamespaceManager, "xsd:date");
            //House Seat
            generateTripleSeat(result, oldGraph, subject);

            return result;
        }

        private void generateTripleSeat(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri seatUri;
            IUriNode seatTypeNode = graph.CreateUriNode("parl:HouseSeat");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            logger.Verbose("Check seat");
            IEnumerable<Triple> existingSeat = oldGraph.GetTriplesWithPredicateObject(rdfTypeNode, seatTypeNode);
            if ((existingSeat != null) && (existingSeat.Any()))
            {
                seatUri = ((IUriNode)existingSeat.SingleOrDefault().Subject).Uri;
                logger.Verbose($"Found seat ({seatUri})");
            }
            else
            {
                string seatId = new IdGenerator.IdMaker().MakeId();
                seatUri = new Uri(seatId);
                logger.Verbose($"New seat ({seatUri})");
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

        private void generateTripleHouse(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri houseUri;
            string houseName = "House of Commons";
            IUriNode housePredicateNode = graph.CreateUriNode("parl:houseSeatHasHouse");
            IEnumerable<Triple> existingHouse = oldGraph.GetTriplesWithSubjectPredicate(subject, housePredicateNode);
            if ((existingHouse != null) && (existingHouse.Any()))
            {
                houseUri = ((IUriNode)existingHouse.SingleOrDefault().Object).Uri;
                logger.Verbose($"Found house ({houseUri})");
            }
            else
            {
                houseUri = IdRetrieval.GetSubject("House", "houseName", houseName, false, logger);
                if (houseUri == null)
                {
                    throw new Exception($"Could not found house ({houseName})");
                }
            }
            graph.Assert(subject, housePredicateNode, graph.CreateUriNode(houseUri));
        }
    }
}
