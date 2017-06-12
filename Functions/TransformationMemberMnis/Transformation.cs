using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions.TransformationMemberMnis
{
    public class Transformation :BaseTransformation<Settings>
    {
        public override IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, Settings settings)
        {
            Graph result = new Graph();
            result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));
            result.NamespaceMap.AddNamespace("ex", new Uri("http://example.com/"));

            telemetryClient.TrackTrace("Generate triples", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IUriNode subject = result.CreateUriNode(subjectUri);
            IUriNode rdfTypeNode = result.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            //Person
            result.Assert(subject, rdfTypeNode, result.CreateUriNode("parl:Person"));
            TripleGenerator.GenerateTriple(result, subject, "parl:personDateOfBirth", doc, "atom:entry/atom:content/m:properties/d:DateOfBirth", settings.SourceXmlNamespaceManager, "xsd:date");
            TripleGenerator.GenerateTriple(result, subject, "parl:personGivenName", doc, "atom:entry/atom:content/m:properties/d:Forename", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personOtherNames", doc, "atom:entry/atom:content/m:properties/d:MiddleNames", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personFamilyName", doc, "atom:entry/atom:content/m:properties/d:Surname", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:F31CBD81AD8343898B49DC65743F0BDF", doc, "atom:entry/atom:content/m:properties/d:NameDisplayAs", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:A5EE13ABE03C4D3A8F1A274F57097B6C", doc, "atom:entry/atom:content/m:properties/d:NameListAs", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "ex:D79B0BAC513C4A9A87C9D5AFF1FC632F", doc, "atom:entry/atom:content/m:properties/d:NameFullTitle", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personPimsId", doc, "atom:entry/atom:content/m:properties/d:Pims_Id", settings.SourceXmlNamespaceManager);
            TripleGenerator.GenerateTriple(result, subject, "parl:personDodsId", doc, "atom:entry/atom:content/m:properties/d:Dods_Id", settings.SourceXmlNamespaceManager);

            //DeceasedPerson
            TripleGenerator.GenerateTriple(result, subject, "parl:personDateOfDeath", doc, "atom:entry/atom:content/m:properties/d:DateOfDeath", settings.SourceXmlNamespaceManager, "xsd:date");
            //MnisPerson
            TripleGenerator.GenerateTriple(result, subject, "parl:personMnisId", doc, "atom:entry/atom:content/m:properties/d:Member_Id", settings.SourceXmlNamespaceManager);
            //GenderIdentity
            XElement genderElement = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:Gender", settings.SourceXmlNamespaceManager);
            string genderMnisId=null;
            if ((genderElement != null) && (string.IsNullOrWhiteSpace(genderElement.Value) == false))
                genderMnisId = genderElement.Value;
            generateTripleGenderIdentity(result, oldGraph, subject, genderMnisId);
            //Incumbency
            generateTripleIncumbency(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);
            //Party membership
            generateTriplePartyMembership(result, oldGraph, subject, doc, settings.SourceXmlNamespaceManager);

            return result;
        }

        private void generateTripleGenderIdentity(IGraph graph, IGraph oldGraph, IUriNode subject, string genderMnisId)
        {
            Uri genderIdentityUri;
            IUriNode genderIdenitytTypeNode = graph.CreateUriNode("parl:GenderIdentity");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            telemetryClient.TrackTrace("Check gender identity", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingGenderIdentity = oldGraph.GetTriplesWithPredicateObject(rdfTypeNode, genderIdenitytTypeNode);
            if ((existingGenderIdentity != null) && (existingGenderIdentity.Any()))
            {
                genderIdentityUri = ((IUriNode)existingGenderIdentity.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found gender identity {genderIdentityUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string genderIdentityId = new IdGenerator.IdMaker().MakeId();
                genderIdentityUri = new Uri(genderIdentityId);
                telemetryClient.TrackTrace($"New gender identity {genderIdentityUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode genderIdentityNode = graph.CreateUriNode(genderIdentityUri);

            graph.Assert(genderIdentityNode, rdfTypeNode, genderIdenitytTypeNode);
            graph.Assert(genderIdentityNode, graph.CreateUriNode("parl:genderIdentityHasPerson"), subject);
            generateTripleGender(graph, oldGraph, genderIdentityNode, genderMnisId);
        }

        private void generateTripleGender(IGraph graph, IGraph oldGraph, IUriNode subject, string genderMnisId)
        {
            Uri genderUri;
            IUriNode genderPredicateNode = graph.CreateUriNode("parl:genderIdentityHasGender");

            telemetryClient.TrackTrace($"Check gender", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            IEnumerable<Triple> existingGender = oldGraph.GetTriplesWithSubjectPredicate(subject, genderPredicateNode);
            if ((existingGender != null) && (existingGender.Any()))
            {
                genderUri = ((IUriNode)existingGender.SingleOrDefault().Object).Uri;
                telemetryClient.TrackTrace($"Found gender {genderUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                genderUri = IdRetrieval.GetSubject("Gender", "genderMnisId", genderMnisId, false, telemetryClient);
                if (genderUri == null)
                {
                    throw new Exception($"{subject.Uri}: Could not found gender for {genderMnisId}");
                }
            }
            graph.Assert(subject, genderPredicateNode, graph.CreateUriNode(genderUri));
        }

        private void generateTripleIncumbency(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager xmlNamespaceManager)
        {
            XElement lordsType;
            XElement startDateElement;
            XElement endDateElement;
            IUriNode incumbencyNode;
            HashSet<string> dates = new HashSet<string>();
            string seatCommand = @"
        construct {
        	?id a parl:HouseSeat.
        }
        where {
        	?id a parl:HouseSeat;
        		parl:houseSeatHasConstituencyGroup ?houseSeatHasConstituencyGroup.
        	?houseSeatHasConstituencyGroup parl:constituencyGroupMnisId @constituencyGroupMnisId.
        }";
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode dataTypeUri = graph.CreateUriNode("xsd:date");
            IUriNode housePredicateNode = graph.CreateUriNode("parl:houseIncumbencyHasHouse");
            IUriNode incumbencyTypePredicateNode = graph.CreateUriNode("parl:houseIncumbencyHasHouseIncumbencyType");

            XElement house = doc.XPathSelectElement("atom:entry/atom:content/m:properties/d:House", xmlNamespaceManager);
            if (house.Value == "Lords")
            {
                foreach (XElement element in doc.XPathSelectElements($"atom:entry/atom:link[@title='MemberLordsMembershipTypes']/m:inline/atom:feed/atom:entry", xmlNamespaceManager))
                {
                    lordsType = element.XPathSelectElement("atom:content/m:properties/d:LordsMembershipType_Id", xmlNamespaceManager);
                    startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", xmlNamespaceManager);
                    endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", xmlNamespaceManager);
                    if (dates.Contains(startDateElement.Value) == true)
                    {
                        telemetryClient.TrackTrace($"Duplicate incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        continue;
                    }
                    dates.Add(startDateElement.Value);
                    if ((string.IsNullOrWhiteSpace(endDateElement.Value)) ||
                        (Convert.ToDateTime(startDateElement.Value) <= Convert.ToDateTime(endDateElement.Value)))
                    {
                        telemetryClient.TrackTrace($"House incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        incumbencyNode = generateIncumbency(graph, oldGraph, subject, startDateElement, endDateElement);
                        IEnumerable<Triple> existingHouses = oldGraph.GetTriplesWithSubjectPredicate(incumbencyNode, housePredicateNode);
                        Uri houseUri = null;
                        if ((existingHouses != null) && (existingHouses.Any()))
                        {
                            houseUri = ((IUriNode)existingHouses.SingleOrDefault().Object).Uri;
                        }
                        else
                        {
                            houseUri = IdRetrieval.GetSubject("House", "houseName", "House of Lords", false, telemetryClient);
                        }
                        IEnumerable<Triple> existingTypes = oldGraph.GetTriplesWithSubjectPredicate(incumbencyNode, incumbencyTypePredicateNode);
                        Uri incumbencyTypeUri = null;
                        if ((existingTypes != null) && (existingTypes.Any()))
                        {
                            incumbencyTypeUri = ((IUriNode)existingTypes.SingleOrDefault().Object).Uri;
                        }
                        else
                        {
                            incumbencyTypeUri = IdRetrieval.GetSubject("HouseIncumbencyType", "houseIncumbencyTypeMnisId", lordsType.Value, false, telemetryClient);
                        }
                        graph.Assert(incumbencyNode, housePredicateNode, graph.CreateUriNode(houseUri));
                        graph.Assert(incumbencyNode, incumbencyTypePredicateNode, graph.CreateUriNode(incumbencyTypeUri));
                    }
                }
            }

            foreach (XElement element in doc.XPathSelectElements("atom:entry/atom:link[@title='MemberConstituencies']/m:inline/atom:feed/atom:entry", xmlNamespaceManager))
            {
                startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", xmlNamespaceManager);
                telemetryClient.TrackTrace($"Seat incumbency ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", xmlNamespaceManager);
                incumbencyNode = generateIncumbency(graph, oldGraph, subject, startDateElement, endDateElement);
                bool hasEndDate = (endDateElement != null && string.IsNullOrWhiteSpace(endDateElement.Value) == false);
                generateSeatIncumbencyParliamentPeriod(graph, incumbencyNode, startDateElement, endDateElement);
                telemetryClient.TrackTrace($"Check seat ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                Uri houseSeatUri;
                XElement constituencyElement = element.XPathSelectElement("atom:link[@title='Constituency']/m:inline/atom:entry/atom:content/m:properties", xmlNamespaceManager);
                if (constituencyElement != null)
                {
                    string constituencyId = constituencyElement.XPathSelectElement("d:Constituency_Id", xmlNamespaceManager).Value;
                    SparqlParameterizedString seatSparql = new SparqlParameterizedString(seatCommand);
                    seatSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                    seatSparql.SetLiteral("constituencyGroupMnisId", constituencyId);
                    houseSeatUri = IdRetrieval.GetSubject(seatSparql.ToString(), false, telemetryClient);
                    if (houseSeatUri != null)
                    {
                        telemetryClient.TrackTrace($"House seat ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                        graph.Assert(incumbencyNode, graph.CreateUriNode("parl:seatIncumbencyHasHouseSeat"), graph.CreateUriNode(houseSeatUri));
                    }
                }
            }
        }

        private IUriNode generateIncumbency(IGraph graph, IGraph oldGraph, IUriNode subject, XElement startDateElement, XElement endDateElement)
        {
            IUriNode dataTypeNode = graph.CreateUriNode("xsd:date");
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            string startDate = giveMeDatePart(startDateElement);
            string endDate = giveMeDatePart(endDateElement);
            INode startDateNode = graph.CreateLiteralNode(startDate, dataTypeNode.Uri);
            IUriNode startDatePredicate = graph.CreateUriNode("parl:incumbencyStartDate");

            IEnumerable<Triple> existingIncumbencies = oldGraph.GetTriplesWithPredicateObject(startDatePredicate, startDateNode);
            Uri incumbencyUri = null;
            if ((existingIncumbencies != null) && (existingIncumbencies.Any()))
            {
                incumbencyUri = ((IUriNode)existingIncumbencies.SingleOrDefault().Subject).Uri;
                telemetryClient.TrackTrace($"Found incumbency {incumbencyUri} ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            else
            {
                string incumbencyId = new IdGenerator.IdMaker().MakeId();
                incumbencyUri = new Uri(incumbencyId);
                telemetryClient.TrackTrace($"New incumbency {incumbencyUri} ({startDateElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            IUriNode incumbencyNode = graph.CreateUriNode(incumbencyUri);
            graph.Assert(incumbencyNode, rdfTypeNode, graph.CreateUriNode("parl:Incumbency"));
            graph.Assert(incumbencyNode, graph.CreateUriNode("parl:incumbencyHasMember"), subject);
            graph.Assert(incumbencyNode, startDatePredicate, startDateNode);
            if (string.IsNullOrWhiteSpace(endDate) == false)
                graph.Assert(incumbencyNode, graph.CreateUriNode("parl:incumbencyEndDate"), graph.CreateLiteralNode(endDate, dataTypeNode.Uri));

            return incumbencyNode;
        }

        private void generateSeatIncumbencyParliamentPeriod(IGraph graph, IUriNode seatIncumbency, XElement startDateElement, XElement endDateElement)
        {
            IUriNode dataTypeNode = graph.CreateUriNode("xsd:date");
            string sparqlCommand = @"
        construct {
            ?parliamentPeriod a parl:ParliamentPeriod.
        }where { 
            ?parliamentPeriod a parl:ParliamentPeriod;
                parl:parliamentPeriodStartDate ?parliamentPeriodStartDate.
            optional {?parliamentPeriod parl:parliamentPeriodEndDate ?parliamentPeriodEndDate}
            filter ((?parliamentPeriodStartDate<=@startDate && ?parliamentPeriodEndDate>=@startDate) ||
    	            (?parliamentPeriodStartDate<=@endDate && ?parliamentPeriodEndDate>=@endDate) ||
                    (?parliamentPeriodStartDate>@startDate && ?parliamentPeriodEndDate<@endDate) ||
                    (bound(?parliamentPeriodEndDate)=false && false=@hasEndDate))
        }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(sparqlCommand);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            string startDate = giveMeDatePart(startDateElement);
            string endDate = giveMeDatePart(endDateElement);
            sparql.SetLiteral("startDate", startDate, dataTypeNode.Uri);
            sparql.SetLiteral("endDate", endDate??string.Empty, dataTypeNode.Uri);
            sparql.SetLiteral("hasEndDate", string.IsNullOrWhiteSpace(endDate)==false);
            Uri parliamentPeriodUri = IdRetrieval.GetSubject(sparql.ToString(), false, telemetryClient);
            if (parliamentPeriodUri != null)
            {
                telemetryClient.TrackTrace($"Parliament period seat ({startDate})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                graph.Assert(seatIncumbency, graph.CreateUriNode("parl:seatIncumbencyHasParliamentPeriod"), graph.CreateUriNode(parliamentPeriodUri));
            }
        }

        private string giveMeDatePart(XElement element)
        {
            if ((element == null) || (string.IsNullOrWhiteSpace(element.Value)))
                return null;
            string date = element.Value;
            if (date.IndexOf("T") > 0)
                date = date.Substring(0, date.IndexOf("T"));
            return date;
        }

        private void generateTriplePartyMembership(IGraph graph, IGraph oldGraph, IUriNode subject, XDocument doc, XmlNamespaceManager namespaceManager)
        {
            Uri partyUri = null;
            IUriNode partyMembershipNode = null;
            IUriNode rdfTypeNode = graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            IUriNode startDatePredicate = graph.CreateUriNode("parl:partyMembershipStartDate");
            IUriNode endDatePredicate = graph.CreateUriNode("parl:partyMembershipEndDate");
            IUriNode hasPartyPredicate = graph.CreateUriNode("parl:partyMembershipHasParty");

            foreach (XElement element in doc.XPathSelectElements("atom:entry/atom:link[@title='MemberParties']/m:inline/atom:feed/atom:entry", namespaceManager))
            {
                XElement partyElement = element.XPathSelectElement("atom:content/m:properties/d:Party_Id", namespaceManager);
                telemetryClient.TrackTrace($"Party membership ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                XElement startDateElement = element.XPathSelectElement("atom:content/m:properties/d:StartDate", namespaceManager);
                XElement endDateElement = element.XPathSelectElement("atom:content/m:properties/d:EndDate", namespaceManager);


                string startDate = giveMeDatePart(startDateElement);
                string endDate = giveMeDatePart(endDateElement);
                IUriNode dataTypeUri = graph.CreateUriNode("xsd:date");

                partyUri = IdRetrieval.GetSubject("Party", "partyMnisId", partyElement.Value, false, telemetryClient);
                if (partyUri == null)
                {
                    telemetryClient.TrackTrace($"Party not found ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    continue;
                }
                INode startDateNode = graph.CreateLiteralNode(startDate, dataTypeUri.Uri);
                IUriNode hasPartyNode = graph.CreateUriNode(partyUri);
                IEnumerable<Triple> existingMembershipByDate = oldGraph.GetTriplesWithPredicateObject(startDatePredicate, startDateNode);
                partyMembershipNode = null;
                if ((existingMembershipByDate != null) && (existingMembershipByDate.Any()))
                {
                    IEnumerable<Triple> existingMembershipByParty = oldGraph.GetTriplesWithPredicateObject(hasPartyPredicate, hasPartyNode);
                    if ((existingMembershipByParty != null) && (existingMembershipByParty.Any()))
                    {
                        IEnumerable<IUriNode> subjectsByDate = existingMembershipByDate.Select(t => (IUriNode)t.Subject);
                        IEnumerable<IUriNode> subjectsByParty = existingMembershipByParty.Select(t => (IUriNode)t.Subject);
                        partyMembershipNode = subjectsByDate.Intersect(subjectsByParty).SingleOrDefault();
                    }
                }
                if (partyMembershipNode == null)
                {
                    string partyMembershipId = new IdGenerator.IdMaker().MakeId();
                    partyMembershipNode = graph.CreateUriNode(new Uri(partyMembershipId));
                    telemetryClient.TrackTrace($"New party membership {partyMembershipNode.Uri} ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                }
                else
                    telemetryClient.TrackTrace($"Found existing party membership {partyMembershipNode.Uri} ({partyElement.Value})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                graph.Assert(partyMembershipNode, rdfTypeNode, graph.CreateUriNode("parl:PartyMembership"));
                graph.Assert(partyMembershipNode, graph.CreateUriNode("parl:partyMembershipHasPartyMember"), subject);
                graph.Assert(partyMembershipNode, graph.CreateUriNode("parl:partyMembershipHasParty"), graph.CreateUriNode(partyUri));
                graph.Assert(partyMembershipNode, startDatePredicate, startDateNode);
                if (string.IsNullOrWhiteSpace(endDate) == false)
                    graph.Assert(partyMembershipNode, endDatePredicate, graph.CreateLiteralNode(endDate, dataTypeUri.Uri));
            }
        }
    }
}