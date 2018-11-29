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

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "9d21ee83-90a7-4c99-8648-523e5eaee734").Replace("{id}", dataUrl);
        }
    }
}
