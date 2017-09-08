namespace Functions.TransformationPartyMnis
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
                ?s a parl:Party.
            }
            where{
                ?s parl:partyMnisId @partyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?party parl:partyMnisId ?partyMnisId;
                parl:partyName ?partyName.
        }
        where {
            bind(@subject as ?party)
            ?party parl:partyMnisId ?partyMnisId.
            optional {?party parl:partyName ?partyName}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Party_Id,Name";
        }
    }
}
