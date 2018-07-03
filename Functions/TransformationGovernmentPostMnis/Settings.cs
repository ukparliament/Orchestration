namespace Functions.TransformationGovernmentPostMnis
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
                ?s a parl:GovernmentPosition.
            }
            where{
                ?s parl:governmentPositionMnisId @governmentPositionMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?governmentPosition a parl:GovernmentPosition;
                parl:governmentPositionMnisId ?governmentPositionMnisId;
                parl:positionName ?positionName;
                parl:positionHasGroup ?positionHasGroup.
        }
        where {
            bind(@subject as ?governmentPosition)
        	?governmentPosition parl:governmentPositionMnisId ?governmentPositionMnisId.
            optional {?governmentPosition parl:positionName ?positionName}
            optional {?governmentPosition parl:positionHasGroup ?positionHasGroup}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=GovernmentPost_Id,Name,GovernmentPostDepartments/Department_Id&$expand=GovernmentPostDepartments";
        }
    }
}
