namespace Functions.TransformationPartyMembershipMnis
{
    public class Settings : ITransformationSettings
    {

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
                ?s a parl:PartyMembership.
            }
            where{
                ?s parl:partyMembershipMnisId @partyMembershipMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?partyMembership a parl:PartyMembership;
                parl:partyMembershipMnisId ?partyMembershipMnisId;
                parl:partyMembershipHasPartyMember ?partyMember;
                parl:partyMembershipStartDate ?partyMembershipStartDate;
                parl:partyMembershipEndDate ?partyMembershipEndDate;
        		parl:partyMembershipHasParty ?partyMembershipHasParty.            
        }
        where {
            bind(@subject as ?partyMembership)
            ?partyMembership parl:partyMembershipMnisId ?partyMembershipMnisId.
            optional {?partyMembership parl:partyMembershipHasPartyMember ?partyMember}
            optional {?partyMembership parl:partyMembershipStartDate ?partyMembershipStartDate}
            optional {?partyMembership parl:partyMembershipHasParty ?partyMembershipHasParty}                
            optional {?partyMembership parl:partyMembershipEndDate ?partyMembershipEndDate}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberParty_Id,Party_Id,StartDate,EndDate,Member_Id";
        }
    }
}
