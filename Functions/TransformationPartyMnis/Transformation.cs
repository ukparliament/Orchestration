using System;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationPartyMnis
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
            //Party
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:Party"));
            TripleGenerator.GenerateTriple(result, subject, "parl:partyMnisId", doc, "atom:entry/atom:content/m:properties/d:Party_Id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:partyName", doc, "atom:entry/atom:content/m:properties/d:Name", settings.SourceXmlNamespaceManager);
            return result;
        }
    }
}
