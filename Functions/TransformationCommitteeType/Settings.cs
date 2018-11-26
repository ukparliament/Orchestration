using System;

namespace Functions.TransformationCommitteeType
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
                return null;
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?formalBodyType a parl:FormalBodyType;
                parl:formalBodyTypeName ?formalBodyTypeName.
        }
        where {
            bind(@subject as ?formalBodyType)
            ?formalBodyType parl:formalBodyTypeName ?formalBodyTypeName.
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "f18a9772-f950-4d45-ab90-08d4f592e08c").Replace("{id}", dataUrl);
        }
    }
}
