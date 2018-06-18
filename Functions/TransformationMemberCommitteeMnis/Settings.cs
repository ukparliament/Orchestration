namespace Functions.TransformationMemberCommitteeMnis
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
                ?s a parl:FormalBodyMembership.
            }
            where{
                ?s parl:formalBodyMembershipMnisId @formalBodyMembershipMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?formalBodyMembership a parl:FormalBodyMembership;
                parl:formalBodyMembershipMnisId ?formalBodyMembershipMnisId;
                parl:formalBodyMembershipStartDate ?formalBodyMembershipStartDate;
                parl:formalBodyMembershipEndDate ?formalBodyMembershipEndDate;
                parl:formalBodyMembershipHasFormalBody ?formalBodyMembershipHasFormalBody;
                parl:exOfficioMembershipHasMember ?exOfficioMembershipHasMember;
                parl:formalBodyMembershipHasPerson ?formalBodyMembershipHasPerson.
            ?formalBodyMembershipHasFormalBody a parl:FormalBody.
            ?exOfficioMembershipHasMember a parl:Member.
            ?formalBodyMembershipHasPerson a parl:Person.
        }
        where {
            bind(@subject as ?formalBodyMembership)
            ?formalBodyMembership parl:formalBodyMembershipMnisId ?formalBodyMembershipMnisId.
            optional {?formalBodyMembership parl:formalBodyMembershipStartDate ?formalBodyMembershipStartDate}
            optional {?formalBodyMembership parl:formalBodyMembershipEndDate ?formalBodyMembershipEndDate}
            optional {?formalBodyMembership parl:formalBodyMembershipHasFormalBody ?formalBodyMembershipHasFormalBody}
            optional {?formalBodyMembership parl:exOfficioMembershipHasMember ?exOfficioMembershipHasMember}
            optional {?formalBodyMembership parl:formalBodyMembershipHasPerson ?formalBodyMembershipHasPerson}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberCommittee_Id,Member_Id,Committee_Id,StartDate,EndDate,ExOfficio";
        }
    }
}
