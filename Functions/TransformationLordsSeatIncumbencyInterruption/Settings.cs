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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "1447ca1f-249d-4491-b9a0-9225b5cd505a").Replace("{id}", dataUrl);
        }
    }
}
