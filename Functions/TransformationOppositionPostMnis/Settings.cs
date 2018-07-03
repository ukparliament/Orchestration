namespace Functions.TransformationOppositionPostMnis
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
                ?s a parl:OppositionPosition.
            }
            where{
                ?s parl:oppositionPositionMnisId @oppositionPositionMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?oppositionPosition a parl:OppositionPosition;
                parl:oppositionPositionMnisId ?oppositionPositionMnisId;
                parl:positionName ?positionName.
        }
        where {
            bind(@subject as ?oppositionPosition)
        	?oppositionPosition parl:oppositionPositionMnisId ?oppositionPositionMnisId.
            optional {?oppositionPosition parl:positionName ?positionName}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=OppositionPost_Id,Name";
        }
    }
}
