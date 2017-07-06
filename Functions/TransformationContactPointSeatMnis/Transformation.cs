using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions.TransformationContactPointSeatMnis
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
            //Contact point
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:ContactPoint"));
            TripleGenerator.GenerateTriple(result, subject, "parl:contactPointMnisId", doc, "atom:entry/atom:content/m:properties/d:MemberAddress_Id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:email", doc, "atom:entry/atom:content/m:properties/d:Email", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:faxNumber", doc, "atom:entry/atom:content/m:properties/d:Fax", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:phoneNumber", doc, "atom:entry/atom:content/m:properties/d:Phone", settings.SourceXmlNamespaceManager);
            //Postal Address
            generateAddressTriples(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            //hasContact
            generateTripleHasContact(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);

            return result;
        }

        private void generateAddressTriples(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
        {
            XElement address;
            bool isFound = false;
            logger.Verbose("Generating address");
            IUriNode postalAddressNode = generateTriplePostalAddress(graph, oldGraph, subject);
            int j = 1;
            for (int i = 1; i <= 5; i++)
            {
                address = doc.XPathSelectElement($"atom:entry/atom:content/m:properties/d:Address{i}", namespaceManager);
                if ((address != null) && (string.IsNullOrWhiteSpace(address.Value) == false))
                {
                    graph.Assert(postalAddressNode, graph.CreateUriNode($"parl:addressLine{j}"), graph.CreateLiteralNode(address.Value));
                    isFound = true;
                    j++;
                }
            }
            address = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Postcode", namespaceManager);
            if ((address != null) && (string.IsNullOrWhiteSpace(address.Value) == false))
            {
                graph.Assert(postalAddressNode, graph.CreateUriNode("parl:postCode"), graph.CreateLiteralNode(address.Value));
                isFound = true;
            }
            if (isFound == true)
            {
                graph.Assert(postalAddressNode, graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:PostalAddress"));
                graph.Assert(subject, graph.CreateUriNode("parl:contactPointHasPostalAddress"), postalAddressNode);
            }
        }

        private IUriNode generateTriplePostalAddress(IGraph graph, IGraph oldGraph, IUriNode subject)
        {
            Uri postalAddressUri;

            logger.Verbose("Check postal address");
            IEnumerable<Triple> existingPostalAddress = oldGraph.GetTriplesWithPredicateObject(graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), graph.CreateUriNode("parl:PostalAddress"));
            if ((existingPostalAddress != null) && (existingPostalAddress.Any()))
            {
                postalAddressUri = ((IUriNode)existingPostalAddress.SingleOrDefault().Subject).Uri;
                logger.Verbose($"Found postal address {postalAddressUri}");
            }
            else
            {
                string postalAddressId = new IdGenerator.IdMaker().MakeId();
                postalAddressUri = new Uri(postalAddressId);
                logger.Verbose($"New postal address {postalAddressUri}");
            }
            IUriNode postalAddressNode = graph.CreateUriNode(postalAddressUri);

            return postalAddressNode;
        }

        private void generateTripleHasContact(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            string hasContactCommand = @"
        construct {
            ?id a parl:Incumbency.
        }
        where {
            ?id a parl:Incumbency;
                parl:incumbencyHasMember ?incumbencyHasMember;
                parl:incumbencyStartDate ?incumbencyStartDate.
            ?incumbencyHasMember parl:personMnisId @personMnisId.
        } order by desc(?incumbencyStartDate) limit 1";
            Uri hasContactUri;
            IUriNode hasContactPredicateNode = graph.CreateUriNode("parl:incumbencyHasContactPoint");

            XElement mnisId = doc.XPathSelectElement($"atom:entry/atom:content/m:properties/d:Member_Id", xmlNamespaceManager);
            if ((mnisId == null) || (string.IsNullOrWhiteSpace(mnisId.Value) == true))
            {
                throw new Exception($"{subject.Uri}: No member info found");
            }
            SparqlParameterizedString hasContactSparql = new SparqlParameterizedString(hasContactCommand);
            hasContactSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            hasContactSparql.SetLiteral("personMnisId", mnisId.Value);
            hasContactUri = IdRetrieval.GetSubject(hasContactSparql.ToString(), false, logger);
            if (hasContactUri != null)
            {
                graph.Assert(graph.CreateUriNode(hasContactUri), hasContactPredicateNode, subject);
            }
            else
                logger.Verbose("No contact found");
        }
    }
}
