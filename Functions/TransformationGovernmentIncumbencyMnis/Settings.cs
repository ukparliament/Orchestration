namespace Functions.TransformationGovernmentIncumbencyMnis
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
                ?s a parl:GovernmentIncumbency.
            }
            where{
                ?s parl:governmentIncumbencyMnisId @governmentIncumbencyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?governmentIncumbency a parl:GovernmentIncumbency;
                parl:governmentIncumbencyHasGovernmentPerson ?person;
                parl:governmentIncumbencyMnisId ?governmentIncumbencyMnisId;
                parl:incumbencyStartDate ?governmentIncumbencyStartDate;
                parl:incumbencyEndDate ?governmentIncumbencyEndDate;
                parl:governmentIncumbencyHasGovernmentPosition ?governmentPosition.
        }
        where {
            bind(@subject as ?governmentIncumbency)
            ?governmentIncumbency parl:governmentIncumbencyMnisId ?governmentIncumbencyMnisId.
            optional {?governmentIncumbency parl:governmentIncumbencyHasGovernmentPerson ?person}
            optional {?governmentIncumbency parl:incumbencyStartDate ?governmentIncumbencyStartDate}
            optional {?governmentIncumbency parl:incumbencyEndDate ?governmentIncumbencyEndDate}
            optional {?governmentIncumbency parl:governmentIncumbencyHasGovernmentPosition ?governmentPosition}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberGovernmentPost_Id,Member_Id,GovernmentPost_Id,StartDate,EndDate";
        }
    }
}
