namespace Functions.TransformationOppositionIncumbencyMnis
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
                ?s a parl:OppositionIncumbency.
            }
            where{
                ?s parl:oppositionIncumbencyMnisId @oppositionIncumbencyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?oppositionIncumbency a parl:OppositionIncumbency;
                parl:oppositionIncumbencyHasOppositionPerson ?person;
                parl:oppositionIncumbencyMnisId ?oppositionIncumbencyMnisId;
                parl:incumbencyStartDate ?oppositionIncumbencyStartDate;
                parl:incumbencyEndDate ?oppositionIncumbencyEndDate;
                parl:oppositionIncumbencyHasOppositionPosition ?oppositionPosition.
        }
        where {
            bind(@subject as ?oppositionIncumbency)
            ?oppositionIncumbency parl:oppositionIncumbencyMnisId ?oppositionIncumbencyMnisId.
            optional {?oppositionIncumbency parl:oppositionIncumbencyHasOppositionPerson ?person}
            optional {?oppositionIncumbency parl:incumbencyStartDate ?oppositionIncumbencyStartDate}
            optional {?oppositionIncumbency parl:incumbencyEndDate ?oppositionIncumbencyEndDate}
            optional {?oppositionIncumbency parl:oppositionIncumbencyHasOppositionPosition ?oppositionPosition}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberOppositionPost_Id,Member_Id,OppositionPost_Id,StartDate,EndDate";
        }
    }
}
