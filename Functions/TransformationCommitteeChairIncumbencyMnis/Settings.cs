namespace Functions.TransformationCommitteeChairIncumbencyMnis
{
    public class Settings :ITransformationSettings
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
                ?s a parl:Incumbency.
            }
            where{
                ?s parl:formalBodyChairIncumbencyMnisId @formalBodyChairIncumbencyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?chairship a parl:Incumbency;
                parl:formalBodyChairIncumbencyMnisId ?formalBodyChairIncumbencyMnisId;
                parl:incumbencyStartDate ?incumbencyStartDate;
                parl:incumbencyEndDate ?incumbencyEndDate;
                parl:incumbencyHasPosition ?position;
                parl:incumbencyHasPerson ?incumbencyHasPerson.
        }
        where {
            bind(@subject as ?chairship)
        	?chairship parl:formalBodyChairIncumbencyMnisId ?formalBodyChairIncumbencyMnisId.
            optional {?chairship parl:incumbencyStartDate ?incumbencyStartDate}
            optional {?chairship parl:incumbencyEndDate ?incumbencyEndDate}
            optional {?chairship parl:incumbencyHasPosition ?position}
            optional {?chairship parl:incumbencyHasPerson ?incumbencyHasPerson}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberCommitteeChair_Id,StartDate,EndDate,MemberCommittee/Member_Id,MemberCommittee/Committee_Id&$expand=MemberCommittee";
        }
    }
}
