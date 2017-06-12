using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationMemberMnis
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationMemberMnis";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                sourceXmlNamespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                sourceXmlNamespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                sourceXmlNamespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Person.
            }
            where{
                ?s a parl:Person; 
                    parl:personMnisId @personMnisId.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"personMnisId","atom:entry/atom:content/m:properties/d:Member_Id" }
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?person a parl:Person;
        		parl:personDateOfBirth ?personDateOfBirth;
        		parl:personGivenName ?personGivenName;
        		parl:personOtherNames ?personOtherNames;
        		parl:personFamilyName ?personFamilyName;
        		parl:personDateOfDeath ?personDateOfDeath;
                <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs;
                <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs;
                <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle;
        		parl:personMnisId ?personMnisId;
                parl:personPimsId ?personPimsId;
                parl:personDodsId ?personDodsId.
            ?genderIdentity a parl:GenderIdentity;
                parl:genderIdentityHasGender ?gender;
                parl:genderIdentityHasPerson ?person.
            ?incumbency a parl:Incumbency;
        		parl:incumbencyHasMember ?person;
                parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat;
                parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod;
        		parl:incumbencyStartDate ?incumbencyStartDate;
        		parl:incumbencyEndDate ?incumbencyEndDate;
                parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
            ?partyMembership a parl:PartyMembership;
        		parl:partyMembershipStartDate ?partyMembershipStartDate;
                parl:partyMembershipEndDate ?partyMembershipEndDate;
        		parl:partyMembershipHasParty ?partyMembershipHasParty;
        		parl:partyMembershipHasPartyMember ?person.
        }
        where {
            bind(@subject as ?person)
            ?person a parl:Person;
                parl:personMnisId ?personMnisId.
            optional {?person parl:personPimsId ?personPimsId}
            optional {?person parl:personDodsId ?personDodsId}
            optional {?person parl:personDateOfBirth ?personDateOfBirth}
            optional {?person parl:personGivenName ?personGivenName}
            optional {?person parl:personOtherNames ?personOtherNames}
            optional {?person parl:personFamilyName ?personFamilyName}
            optional {?person parl:personDateOfDeath ?personDateOfDeath}
            optional {?person <http://example.com/F31CBD81AD8343898B49DC65743F0BDF> ?personNameDisplayAs}
            optional {?person <http://example.com/A5EE13ABE03C4D3A8F1A274F57097B6C> ?personNameListAs}
            optional {?person <http://example.com/D79B0BAC513C4A9A87C9D5AFF1FC632F> ?personNameFullTitle}
            optional {
                ?genderIdentity a parl:GenderIdentity;
                    parl:genderIdentityHasGender ?gender;
                    parl:genderIdentityHasPerson ?person.
                ?gender parl:genderMnisId @genderMnisId.
            }
            optional {
                ?incumbency a parl:Incumbency;
                    parl:incumbencyHasMember ?person.
                optional {?incumbency parl:incumbencyStartDate ?incumbencyStartDate}
                optional {?incumbency parl:incumbencyEndDate ?incumbencyEndDate}
                optional {?incumbency parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat}
                optional {?incumbency parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod}
                optional {
                    ?incumbency parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                        parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
                    ?houseIncumbencyHasHouse parl:houseName ""House of Lords"".
                }
            }
            optional {
                ?partyMembership a parl:PartyMembership;
                    parl:partyMembershipStartDate ?partyMembershipStartDate;
                    parl:partyMembershipHasParty ?partyMembershipHasParty;
                    parl:partyMembershipHasPartyMember ?person.
                optional {?partyMembership parl:partyMembershipEndDate ?partyMembershipEndDate}
            }
        }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"genderMnisId","atom:entry/atom:content/m:properties/d:Gender" }
                };
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Surname,MiddleNames,Forename,NameDisplayAs,NameListAs,NameFullTitle,DateOfBirth,DateOfDeath,Gender,Member_Id,House,StartDate,EndDate,Dods_Id,Pims_Id,MemberConstituencies/StartDate,MemberConstituencies/EndDate,MemberConstituencies/EndDate,MemberConstituencies/Constituency/Constituency_Id,MemberConstituencies/Constituency/StartDate,MemberConstituencies/Constituency/EndDate,MemberParties/Party_Id,MemberParties/StartDate,MemberParties/EndDate,MemberLordsMembershipTypes/StartDate,MemberLordsMembershipTypes/EndDate,MemberLordsMembershipTypes/LordsMembershipType_Id&$expand=MemberConstituencies,MemberConstituencies/Constituency,MemberParties,MemberLordsMembershipTypes";
        }
    }
}
