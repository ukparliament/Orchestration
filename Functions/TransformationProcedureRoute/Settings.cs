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

        public string ParameterizedString(string dataUrl)
        {
            return $@"select r.TripleStoreId, t.ProcedureRouteTypeName,
                    fs.TripleStoreId as FromStep, ts.TripleStoreId as ToStep, cast(0 as bit) as IsDeleted from ProcedureRoute r
                join ProcedureRouteType t on t.Id=r.ProcedureRouteTypeId
                join ProcedureStep fs on fs.Id=r.FromProcedureStepId
                join ProcedureStep ts on ts.Id=r.ToProcedureStepId
                where r.Id={dataUrl}
                union
				select r.TripleStoreId, null as ProcedureRouteTypeName,
                    null as FromStep, null as ToStep, cast(1 as bit) as IsDeleted from DeletedProcedureRoute r
                where r.Id={dataUrl};
                select p.TripleStoreId from ProcedureRouteProcedure rp
                join ProcedureRoute r on r.Id=rp.ProcedureRouteId
                join [Procedure] p on p.Id= rp.ProcedureId
                where r.Id={dataUrl}";
        }
    }
}
