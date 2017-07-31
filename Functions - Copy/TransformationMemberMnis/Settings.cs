namespace Functions.TransformationMemberMnis
{
    public class Settings : ITransformationSettings
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

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Person.
            }
            where{
                ?s parl:personMnisId @personMnisId.
            }";
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
                parl:personDodsId ?personDodsId;
                parl:personHasGenderIdentity ?genderIdentity;
                parl:memberHasIncumbency ?incumbency;
                parl:partyMemberHasPartyMembership ?partyMembership.
            ?genderIdentity a parl:GenderIdentity;
                parl:genderIdentityHasGender ?gender.
            ?incumbency a parl:Incumbency;
                parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat;
                parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod;
        		parl:incumbencyStartDate ?incumbencyStartDate;
        		parl:incumbencyEndDate ?incumbencyEndDate;
                parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
            ?partyMembership a parl:PartyMembership;
                parl:partyMembershipStartDate ?partyMembershipStartDate;
                parl:partyMembershipEndDate ?partyMembershipEndDate;
        		parl:partyMembershipHasParty ?partyMembershipHasParty.
        }
        where {
            bind(@subject as ?person)
            ?person parl:personMnisId ?personMnisId.
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
                bind(@gender as ?gender)
                ?genderIdentity parl:genderIdentityHasGender ?gender;
                    parl:genderIdentityHasPerson ?person.
            }
            optional {
                ?incumbency parl:incumbencyHasMember ?person.
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
                ?partyMembership parl:partyMembershipStartDate ?partyMembershipStartDate;
                    parl:partyMembershipHasParty ?partyMembershipHasParty;
                    parl:partyMembershipHasPartyMember ?person.
                optional {?partyMembership parl:partyMembershipEndDate ?partyMembershipEndDate}
            }
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Surname,MiddleNames,Forename,NameDisplayAs,NameListAs,NameFullTitle,DateOfBirth,DateOfDeath,Gender,Member_Id,House,StartDate,EndDate,Dods_Id,Pims_Id,MemberConstituencies/StartDate,MemberConstituencies/EndDate,MemberConstituencies/EndDate,MemberConstituencies/Constituency/Constituency_Id,MemberConstituencies/Constituency/StartDate,MemberConstituencies/Constituency/EndDate,MemberParties/Party_Id,MemberParties/StartDate,MemberParties/EndDate,MemberLordsMembershipTypes/StartDate,MemberLordsMembershipTypes/EndDate,MemberLordsMembershipTypes/LordsMembershipType_Id&$expand=MemberConstituencies,MemberConstituencies/Constituency,MemberParties,MemberLordsMembershipTypes";
        }
    }
}
