using System;

namespace Functions.TransformationProcedureRoute
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
            ?procedureRoute a parl:ProcedureRoute;
        	    parl:procedureRouteHasProcedure ?procedureRouteHasProcedure;
                parl:procedureRouteIsFromProcedureStep ?procedureRouteIsFromProcedureStep;
                parl:procedureRouteIsToProcedureStep ?procedureRouteIsToProcedureStep.
            ?procedureRouteHasProcedure a parl:Procedure.
            ?procedureRouteIsFromProcedureStep a parl:ProcedureStep;
                ?procedureRouteTriggerType ?procedureRoute.
            ?procedureRouteIsToProcedureStep a parl:ProcedureStep.            
        }
        where {
            bind(@subject as ?procedureRoute)
            ?procedureRoute parl:procedureRouteHasProcedure ?procedureRouteHasProcedure.
            optional{?procedureRoute parl:procedureRouteIsFromProcedureStep ?procedureRouteIsFromProcedureStep}
            optional{?procedureRoute parl:procedureRouteIsToProcedureStep ?procedureRouteIsToProcedureStep}
            optional{
                ?procedureRouteIsFromProcedureStep parl:procedureStepCausesCausedProcedureRoute ?procedureRoute.
                bind(parl:procedureStepCausesCausedProcedureRoute as ?procedureRouteTriggerType)
            }
            optional{
                ?procedureRouteIsFromProcedureStep parl:procedureStepAllowsAllowedProcedureRoute ?procedureRoute.
                bind(parl:procedureStepAllowsAllowedProcedureRoute as ?procedureRouteTriggerType)
            }
            optional{
                ?procedureRouteIsFromProcedureStep parl:procedureStepPrecludesPrecludedProcedureRoute ?procedureRoute.
                bind(parl:procedureStepPrecludesPrecludedProcedureRoute as ?procedureRouteTriggerType)
            }
            optional{
                ?procedureRouteIsFromProcedureStep parl:procedureStepRequiresRequiredProcedureRoute ?procedureRoute.
                bind(parl:procedureStepRequiresRequiredProcedureRoute as ?procedureRouteTriggerType)
            }
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "0c651f3a-630a-4ca6-b8c6-3a85f381b41d").Replace("{id}", dataUrl);
        }
    }
}
