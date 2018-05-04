using System;

namespace Functions.TransformationProcedureStep
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
            ?procedureStep a parl:ProcedureStep;
        	    parl:procedureStepName ?procedureStepName;
                parl:procedureStepDescription ?procedureStepDescription;
                parl:procedureStepHasHouse ?procedureStepHasHouse.
            ?procedureStepHasHouse a parl:House.
        }
        where {
            bind(@subject as ?procedureStep)
            ?procedureStep parl:procedureStepName ?procedureStepName.
            optional {?procedureStep parl:procedureStepDescription ?procedureStepDescription}
            optional {?procedureStep parl:procedureStepHasHouse ?procedureStepHasHouse}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "c53aee5a-5dcc-4321-a60a-1432dc7b7bae").Replace("{id}", dataUrl);
        }
    }
}
