using System;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationTerritory
{
    public class Transformation:BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            logger.Verbose("Generate triples");
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //Territory
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:Territory"));
            TripleGenerator.GenerateTriple(result, subject, "parl:territoryGovRegisterId", doc, "root/*/key", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:territoryName", doc, "root/*/item/name", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:territoryOfficialName", doc, "root/*/item/official-name", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:govRegisterTerritoryStartDate", doc, "root/*/item/start-date", settings.SourceXmlNamespaceManager,"xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:govRegisterTerritoryEndDate", doc, "root/*/item/end-date", settings.SourceXmlNamespaceManager, "xsd:date");

            return result;
        }
    }
}
