using System;

namespace Functions.TransformationProcedure
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
            ?procedure a parl:Procedure;
        	    parl:procedureName ?procedureName.
        }
        where {
            bind(@subject as ?procedure)
            ?procedure parl:procedureName ?procedureName.
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "adad783c-971c-4bb3-a07b-28a5068cc1e0").Replace("{id}", dataUrl);
        }
    }
}
