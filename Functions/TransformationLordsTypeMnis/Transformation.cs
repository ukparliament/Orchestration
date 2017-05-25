using System;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationLordsTypeMnis
{
    public class Transformation:BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));

            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //HouseIncumbencyType
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:HouseIncumbencyType"));
            TripleGenerator.GenerateTriple(result, subject, "parl:houseIncumbencyTypeMnisId", doc, "atom:entry/atom:content/m:properties/d:LordsMembershipType_Id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:houseIncumbencyTypeName", doc, "atom:entry/atom:content/m:properties/d:Name", settings.SourceXmlNamespaceManager);
            return result;
        }
    }
}
