namespace Functions.TransformationAnsweringBodyMnis
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
                ?s a parl:AnsweringBody.
            }
            where{
                ?s parl:answeringBodyMnisId @answeringBodyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?answeringBody a parl:MnisAnsweringBody;
                parl:answeringBodyMnisId ?answeringBodyMnisId;
                parl:name ?name.
        }
        where {
            bind(@subject as ?answeringBody)
            ?answeringBody parl:answeringBodyMnisId ?answeringBodyMnisId.
            OPTIONAL {?answeringBody parl:name ?name.}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=AnsweringBody_Id,Department_Id,Name";
        }
    }
}
