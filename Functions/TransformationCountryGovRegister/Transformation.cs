using System;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.TransformationCountryGovRegister
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
            //Country
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:Country"));
            TripleGenerator.GenerateTriple(result, subject, "parl:countryGovRegisterId", doc, "root/*/key", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:countryOfficialName", doc, "root/*/item/official-name", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:countryCitizenNames", doc, "root/*/item/citizen-names", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:govRegisterCountryStartDate", doc, "root/*/item/start-date", settings.SourceXmlNamespaceManager,"xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:govRegisterCountryEndDate", doc, "root/*/item/end-date", settings.SourceXmlNamespaceManager, "xsd:date");

            return result;
        }
    }
}
