﻿using System;

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
            return $@"select r.TripleStoreId, p.TripleStoreId as [Procedure], t.ProcedureRouteTypeName,
                    fs.TripleStoreId as FromStep, ts.TripleStoreId as ToStep, r.IsDeleted from ProcedureRoute r
                join [Procedure] p on p.Id=r.ProcedureId
                join ProcedureRouteType t on t.Id=r.ProcedureRouteTypeId
                join ProcedureStep fs on fs.Id=r.FromProcedureStepId
                join ProcedureStep ts on ts.Id=r.ToProcedureStepId
                where r.Id={dataUrl};";
        }
    }
}
