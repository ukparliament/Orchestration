using System;

namespace Functions.TransformationLordsSeatIncumbencyInterruption
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
            ?incumbencyInterruption a parl:IncumbencyInterruption.
            ?incumbencyInterruption parl:incumbencyInterruptionHasIncumbency ?incumbencyInterruptionHasIncumbency.
            ?incumbencyInterruption parl:incumbencyInterruptionStartDate ?incumbencyInterruptionStartDate.
            ?incumbencyInterruption parl:incumbencyInterruptionEndDate ?incumbencyInterruptionEndDate.
        }
        where {
            bind(@subject as ?incumbencyInterruption)
            ?incumbencyInterruption parl:incumbencyInterruptionHasIncumbency ?incumbencyInterruptionHasIncumbency.
            ?incumbencyInterruption parl:incumbencyInterruptionStartDate ?incumbencyInterruptionStartDate.
            optional {?incumbencyInterruption parl:incumbencyInterruptionEndDate ?incumbencyInterruptionEndDate}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "919f9534-241d-4f78-9e48-5b37e539aa39").Replace("{id}", dataUrl);
        }
    }
}
