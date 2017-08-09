﻿using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationMemberMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IBaseOntology[] TransformSource(string response)
        {
            IMember member = new Member();
            XDocument doc = XDocument.Parse(response);
            XElement personElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            member.PersonDateOfBirth = personElement.Element(d + "DateOfBirth").GetDate();
            member.PersonGivenName = personElement.Element(d + "Forename").GetText();
            member.PersonOtherNames = personElement.Element(d + "MiddleNames").GetText();
            member.PersonFamilyName = personElement.Element(d + "Surname").GetText();
            IMnisPerson mnisPerson = new MnisPerson();
            mnisPerson.PersonMnisId = personElement.Element(d + "Member_Id").GetText();
            IDodsPerson dodsPerson = new DodsPerson();
            dodsPerson.PersonDodsId = personElement.Element(d + "Dods_Id").GetText();
            IPimsPerson pimsPerson = new PimsPerson();
            pimsPerson.PersonPimsId = personElement.Element(d + "Pims_Id").GetText();
            IDeceasedPerson deceasedPerson = new DeceasedPerson();
            deceasedPerson.PersonDateOfDeath = personElement.Element(d + "DateOfDeath").GetDate();
            IPartyMember partyMember = new PartyMember();
            partyMember.PartyMemberHasPartyMembership = generatePartyMembership(doc).ToArray();

            string currentGenderText = personElement.Element(d + "Gender").GetText();
            if (string.IsNullOrWhiteSpace(currentGenderText) == false)
            {
                IGenderIdentity genderIdentity = generateGenderIdentity(currentGenderText);
                member.PersonHasGenderIdentity = new IGenderIdentity[] { genderIdentity };
            }

            List<IIncumbency> incumbencies = new List<IIncumbency>();
            string house = personElement.Element(d + "House").GetText();
            if (house == "Lords")
                incumbencies.AddRange(generateHouseIncumbencies(doc));
            incumbencies.AddRange(generateSeatIncumbencies(doc));
            member.MemberHasIncumbency = incumbencies.ToArray();

            return new IBaseOntology[] { member, mnisPerson, dodsPerson, pimsPerson, deceasedPerson, partyMember };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string personMnisId = deserializedSource.OfType<IMnisPerson>()
                .SingleOrDefault()
                .PersonMnisId;
            return new Dictionary<string, object>()
            {
                { "personMnisId", personMnisId }
            };
        }

        public override Dictionary<string, object> GetKeysForTarget(IBaseOntology[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<IMember>()
                .SingleOrDefault()
                .PersonHasGenderIdentity
                .SingleOrDefault()
                .GenderIdentityHasGender
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "gender", genderUri }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IMember member = source.OfType<IMember>().SingleOrDefault();
            if ((member.PersonHasGenderIdentity != null) && (member.PersonHasGenderIdentity.Any()))
            {
                IGenderIdentity genderIdentity = target.OfType<IGenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    member.PersonHasGenderIdentity.SingleOrDefault().SubjectUri = genderIdentity.SubjectUri;
            }
            if ((member.MemberHasIncumbency != null) && (member.MemberHasIncumbency.Any()))
            {
                IEnumerable<IIncumbency> incumbencies = target.OfType<IIncumbency>();
                if ((incumbencies != null) && (incumbencies.Any()))
                {
                    foreach (IIncumbency incumbency in member.MemberHasIncumbency.Where(i => i.IncumbencyStartDate != null))
                    {
                        IIncumbency foundIncumbency = incumbencies.SingleOrDefault(h => h.IncumbencyStartDate == incumbency.IncumbencyStartDate);
                        if (foundIncumbency != null)
                        {
                            IIncumbency[] matchedIncumbencies = member.MemberHasIncumbency.Where(i => i.SubjectUri == incumbency.SubjectUri).ToArray();
                            foreach (IIncumbency newIncumbency in matchedIncumbencies)
                                newIncumbency.SubjectUri = foundIncumbency.SubjectUri;
                        }
                    }
                }
            }
            IPartyMember partyMember = source.OfType<IPartyMember>().SingleOrDefault();
            if ((partyMember.PartyMemberHasPartyMembership != null) && (partyMember.PartyMemberHasPartyMembership.Any()))
            {
                partyMember.PartyMemberHasPartyMembership = partyMember.PartyMemberHasPartyMembership.ToArray();
                IEnumerable<IPartyMembership> partyMemberships = target.OfType<IPartyMembership>();
                if ((partyMemberships != null) && (partyMemberships.Any()))
                {
                    foreach (IPartyMembership partyMembership in partyMember.PartyMemberHasPartyMembership)
                    {
                        IPartyMembership foundPartyMembership = partyMemberships.SingleOrDefault(h => h.PartyMembershipStartDate == partyMembership.PartyMembershipStartDate);
                        if (foundPartyMembership != null)
                            partyMembership.SubjectUri = partyMemberships.SingleOrDefault(h => h.PartyMembershipStartDate == partyMembership.PartyMembershipStartDate).SubjectUri;
                    }
                }
            }
            foreach (IPerson person in source.OfType<IPerson>())
                person.SubjectUri = subjectUri;
            return source.OfType<IPerson>().ToArray();
        }

        public override IGraph AlterNewGraph(IGraph newGraph, Uri subjectUri, string response)
        {
            XDocument doc = XDocument.Parse(response);
            XElement personElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            string value = personElement.Element(d + "NameDisplayAs").GetText();
            if (string.IsNullOrWhiteSpace(value)==false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/F31CBD81AD8343898B49DC65743F0BDF")), newGraph.CreateLiteralNode(value));
            value = personElement.Element(d + "NameListAs").GetText();
            if (string.IsNullOrWhiteSpace(value) == false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C")), newGraph.CreateLiteralNode(value));
            value = personElement.Element(d + "NameFullTitle").GetText();
            if (string.IsNullOrWhiteSpace(value) == false)
                newGraph.Assert(newGraph.CreateUriNode(subjectUri), newGraph.CreateUriNode(new Uri("http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F")), newGraph.CreateLiteralNode(value));

            return newGraph;
        }

        private IEnumerable<IIncumbency> generateSeatIncumbencies(XDocument doc)
        {
            IEnumerable<XElement> memberIncumbenciesElements = doc.Element(atom + "entry")
                    .Elements(atom + "link")
                    .Where(e => e.Attribute("title").Value == "MemberConstituencies")
                    .SingleOrDefault()
                    .Element(m + "inline")
                    .Element(atom + "feed")
                    .Elements(atom + "entry");
            foreach (XElement element in memberIncumbenciesElements)
            {
                XElement seatIncumbencyElement = element.Element(atom + "content").Element(m + "properties");
                IPastIncumbency incumbency = new PastIncumbency();
                incumbency.SubjectUri = GenerateNewId();
                incumbency.IncumbencyStartDate = seatIncumbencyElement.Element(d + "StartDate").GetDate();
                incumbency.IncumbencyEndDate = seatIncumbencyElement.Element(d + "EndDate").GetDate();
                ISeatIncumbency seatIncumbency = new SeatIncumbency();                
                seatIncumbency.SubjectUri = incumbency.SubjectUri;
                IParliamentPeriod parliamentPeriod = generateSeatIncumbencyParliamentPeriod(incumbency.IncumbencyStartDate.Value, incumbency.IncumbencyEndDate);
                if (parliamentPeriod != null)
                    seatIncumbency.SeatIncumbencyHasParliamentPeriod = new IParliamentPeriod[] { parliamentPeriod };

                XElement constituencyElement = element.Elements(atom + "link")
                    .Where(e => e.Attribute("title").Value == "Constituency")
                    .SingleOrDefault()
                    .Element(m + "inline")
                    .Element(atom + "entry")
                    .Element(atom + "content")
                    .Element(m + "properties")
                    .Element(d + "Constituency_Id");
                string houseSeatCommand = @"
        construct {
        	?id a parl:HouseSeat.
        }
        where {
        	?id parl:houseSeatHasConstituencyGroup ?houseSeatHasConstituencyGroup.
        	?houseSeatHasConstituencyGroup parl:constituencyGroupMnisId @constituencyGroupMnisId.
        }";
                if ((constituencyElement != null) && (string.IsNullOrWhiteSpace(constituencyElement.Value) == false))
                {
                    SparqlParameterizedString seatSparql = new SparqlParameterizedString(houseSeatCommand);
                    seatSparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                    seatSparql.SetLiteral("constituencyGroupMnisId", constituencyElement.Value);
                    Uri houseSeatUri = IdRetrieval.GetSubject(seatSparql.ToString(), false, logger);
                    if (houseSeatUri != null)
                        seatIncumbency.SeatIncumbencyHasHouseSeat = new HouseSeat()
                        {
                            SubjectUri = houseSeatUri
                        };
                }
                yield return incumbency;
                yield return seatIncumbency;
            }
        }

        private IEnumerable<IIncumbency> generateHouseIncumbencies(XDocument doc)
        {
            HashSet<DateTimeOffset> dates = new HashSet<DateTimeOffset>();
            Uri houseUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
            IEnumerable<XElement> lordsIncumbenciesElements = doc.Element(atom + "entry")
                    .Elements(atom + "link")
                    .Where(e => e.Attribute("title").Value == "MemberLordsMembershipTypes")
                    .SingleOrDefault()
                    .Element(m + "inline")
                    .Element(atom + "feed")
                    .Elements(atom + "entry");
            Dictionary<string, string> incumbencyTypes = IdRetrieval.GetSubjectsDictionary("houseIncumbencyTypeMnisId", logger);
            foreach (XElement element in lordsIncumbenciesElements)
            {
                XElement lordIncumbencyElement = element.Element(atom + "content").Element(m + "properties");
                IPastIncumbency incumbency = new PastIncumbency();
                incumbency.SubjectUri = GenerateNewId();
                incumbency.IncumbencyStartDate = lordIncumbencyElement.Element(d + "StartDate").GetDate();
                incumbency.IncumbencyEndDate = lordIncumbencyElement.Element(d + "EndDate").GetDate();
                if (((incumbency.IncumbencyEndDate.HasValue == false) || (incumbency.IncumbencyStartDate <= incumbency.IncumbencyEndDate)) &&
                    (dates.Contains(incumbency.IncumbencyStartDate.Value) == false))
                {
                    dates.Add(incumbency.IncumbencyStartDate.Value);
                    string houseIncumbencyTypeMnisId = lordIncumbencyElement.Element(d + "LordsMembershipType_Id").GetText();
                    IHouseIncumbency houseIncumbency = new HouseIncumbency();
                    houseIncumbency.SubjectUri = incumbency.SubjectUri;
                    if (incumbencyTypes.ContainsKey(houseIncumbencyTypeMnisId))
                        houseIncumbency.HouseIncumbencyHasHouseIncumbencyType = new HouseIncumbencyType()
                        {
                            SubjectUri = new Uri(incumbencyTypes[houseIncumbencyTypeMnisId])
                        };
                    houseIncumbency.HouseIncumbencyHasHouse = new House() { SubjectUri = houseUri };
                    yield return incumbency;
                    yield return houseIncumbency;
                }
            }
        }

        private IGenderIdentity generateGenderIdentity(string currentGenderText)
        {
            IGenderIdentity genderIdentity = new GenderIdentity();
            IGender gender = new Gender();
            genderIdentity.SubjectUri = GenerateNewId();
            gender.SubjectUri = IdRetrieval.GetSubject("genderMnisId", currentGenderText, false, logger);
            genderIdentity.GenderIdentityHasGender = gender;
            return genderIdentity;
        }

        private IParliamentPeriod generateSeatIncumbencyParliamentPeriod(DateTimeOffset startDate, DateTimeOffset? endDate)
        {
            string sparqlCommand = @"
        construct {
            ?parliamentPeriod a parl:ParliamentPeriod.
        }where { 
            ?parliamentPeriod parl:parliamentPeriodStartDate ?parliamentPeriodStartDate.
            optional {?parliamentPeriod parl:parliamentPeriodEndDate ?parliamentPeriodEndDate}
            filter ((?parliamentPeriodStartDate<=@startDate && ?parliamentPeriodEndDate>=@startDate) ||
    	            (?parliamentPeriodStartDate<=@endDate && ?parliamentPeriodEndDate>=@endDate) ||
                    (?parliamentPeriodStartDate>@startDate && ?parliamentPeriodEndDate<@endDate) ||
                    (bound(?parliamentPeriodEndDate)=false && false=@hasEndDate))
        }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(sparqlCommand);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            sparql.SetLiteral("startDate", startDate.ToString("yyyy-MM-dd"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("endDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : string.Empty, new Uri("http://www.w3.org/2001/XMLSchema#date"));
            sparql.SetLiteral("hasEndDate", endDate.HasValue);
            Uri parliamentPeriodUri = null;
            try
            {
                parliamentPeriodUri = IdRetrieval.GetSubject(sparql.ToString(), false, logger);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "Sequence contains more than one element")
                    logger.Warning($"More than one parliamentary period found for incumbency {startDate}-{endDate}");
                else
                    logger.Exception(e);
            }
            if (parliamentPeriodUri != null)
            {
                return new ParliamentPeriod()
                {
                    SubjectUri = parliamentPeriodUri
                };
            }
            else
                return null;
        }

        private IEnumerable<IPartyMembership> generatePartyMembership(XDocument doc)
        {
            IEnumerable<XElement> memberPartiesElements = doc.Element(atom + "entry")
                    .Elements(atom + "link")
                    .Where(e => e.Attribute("title").Value == "MemberParties")
                    .SingleOrDefault()
                    .Element(m + "inline")
                    .Element(atom + "feed")
                    .Elements(atom + "entry");
            Dictionary<string, Uri> partyIds = new Dictionary<string, Uri>();
            foreach (XElement element in memberPartiesElements)
            {
                IPastPartyMembership partyMembership = new PastPartyMembership();
                partyMembership.SubjectUri = GenerateNewId();
                XElement partyElement = element.Element(atom + "content").Element(m + "properties");
                string partyId = partyElement.Element(d + "Party_Id").GetText();
                if (partyIds.ContainsKey(partyId))
                    partyMembership.PartyMembershipHasParty = new Party()
                    {
                        SubjectUri = partyIds[partyId]
                    };
                else
                {
                    Uri partyUri = IdRetrieval.GetSubject("partyMnisId", partyId, false, logger);
                    if (partyUri == null)
                        continue;
                    partyMembership.PartyMembershipHasParty = new Party()
                    {
                        SubjectUri = partyUri
                    };
                    partyIds.Add(partyId, partyUri);
                }
                partyMembership.PartyMembershipStartDate = partyElement.Element(d + "StartDate").GetDate();
                partyMembership.PartyMembershipEndDate = partyElement.Element(d + "EndDate").GetDate();

                yield return partyMembership;
            }
        }
    }
}