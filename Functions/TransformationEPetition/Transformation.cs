using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

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
            //TripleGenerator.GenerateTriple(result, subject, "parl:openedAt", doc, "root/data/id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:background", doc, "root/data/attributes/background", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:additionalDetails", doc, "root/data/attributes/additional_details", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:closedAt", doc, "root/data/attributes/closed_at", settings.SourceXmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:action", doc, "root/data/attributes/action", settings.SourceXmlNamespaceManager);
            return result;
        }

        private void generateTripleGovernmentResponse(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            Uri governmentResponseUri;
            IUriNode governmentResponseNode = graph.CreateUriNode("parl:ePetitionHasGovernmentResponse");
            IEnumerable<Triple> exisitngGovernmentResponses = oldGraph.GetTriplesWithPredicate(governmentResponseNode);
            if ((exisitngGovernmentResponses != null) && (exisitngGovernmentResponses.Any()))
            {
                governmentResponseUri = ((IUriNode)exisitngGovernmentResponses.SingleOrDefault().Object).Uri;
            }
            else
            {
                string governmentResponseId = new IdGenerator.IdMaker().MakeId();
                governmentResponseUri = new Uri(governmentResponseId);
                telemetryClient.TrackTrace($"New government response {governmentResponseUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode governmentResponse = graph.CreateUriNode(governmentResponseUri);
            graph.Assert(subject, governmentResponseNode, governmentResponse);
            TripleGenerator.GenerateTriple((Graph)graph, governmentResponse, "parl:governmentResponseCreatedAt", doc, "root/data/attributes/government_response/created_at", xmlNamespaceManager, "xsd:date");
        }
    }
}
