using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureRoute
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            ProcedureRoute procedureRoute = new ProcedureRoute();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
                procedureRoute.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { procedureRoute };

            string routeType = GetText(row["ProcedureRouteTypeName"]);
            if (string.IsNullOrWhiteSpace(routeType))
                return null;

            if (Enum.TryParse<ProcedureRouteType>(routeType, out ProcedureRouteType procedureRouteKind) == false)
            {
                logger.Warning($"Invalid route type '{routeType}' found");
                return null;
            }

            Uri procedureUri = GiveMeUri(GetText(row["Procedure"]));
            if (procedureUri != null)
                procedureRoute.ProcedureRouteHasProcedure = new List<Procedure>()
                {
                    new Procedure()
                    {
                        Id=procedureUri
                    }
                };
            else
                return null;
            Uri fromStepUri = GiveMeUri(GetText(row["FromStep"]));
            if (fromStepUri != null)
            {
                ProcedureStep fromProcedureStep = new ProcedureStep();
                fromProcedureStep.Id = fromStepUri;
                switch (procedureRouteKind)
                {
                    case ProcedureRouteType.Allows:
                        fromProcedureStep.ProcedureStepAllowsAllowedProcedureRoute = new List<AllowedProcedureRoute>()
                        {
                            new AllowedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                    case ProcedureRouteType.Causes:
                        fromProcedureStep.ProcedureStepCausesCausedProcedureRoute = new List<CausedProcedureRoute>()
                        {
                            new CausedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                    case ProcedureRouteType.Precludes:
                        fromProcedureStep.ProcedureStepPrecludesPrecludedProcedureRoute = new List<PrecludedProcedureRoute>()
                        {
                            new PrecludedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                    case ProcedureRouteType.Requires:
                        fromProcedureStep.ProcedureStepRequiresRequiredProcedureRoute = new List<RequiredProcedureRoute>()
                        {
                            new RequiredProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                }
                procedureRoute.ProcedureRouteIsFromProcedureStep = new List<ProcedureStep>()
                {
                    fromProcedureStep
                };
            }
            else
                return null;
            Uri toStepUri = GiveMeUri(GetText(row["ToStep"]));
            if (toStepUri != null)
                procedureRoute.ProcedureRouteIsToProcedureStep = new List<ProcedureStep>()
                {
                    new ProcedureStep()
                    {
                        Id=toStepUri
                    }
                };
            else
                return null;

            return new BaseResource[] { procedureRoute };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<ProcedureRoute>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
        
    }
}
