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
                return @"
            construct{
                ?s a parl:FormalBodyType.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:formalBodyTypeName ?formalBodyTypeName.
            }";
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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "774e8a14-a613-4948-a645-a434ef6a8f1f").Replace("{id}", dataUrl);
        }
    }
}
