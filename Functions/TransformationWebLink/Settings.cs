using System;

namespace Functions.TransformationWebLink
{
    public class Settings : ITransformationSettings
    {
        public string AcceptHeader
        {
            get
            {
                return string.Empty;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return null;
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?personWebLink a parl:PersonWebLink;
        	    parl:personWebLinkHasPerson ?personWebLinkHasPerson.
        }
        where {
            bind(@subject as ?personWebLink)
            ?personWebLink parl:personWebLinkHasPerson ?personWebLinkHasPerson.
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "7cb6d78b-5f45-49d2-bb77-009f71a83390").Replace("{id}", dataUrl);
        }
    }
}
