using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationProcedureRoute
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IProcedureRoute procedureRoute = new ProcedureRoute();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("TripleStoreId")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    procedureRoute.Id = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            string routeType = ((JValue)((JObject)jsonResponse.SelectToken("RouteType"))?.SelectToken("Value"))?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(routeType))
                return null;

            if (Enum.TryParse<ProcedureRouteType>(routeType, out ProcedureRouteType procedureRouteKind) == false)
            {
                logger.Warning($"Invalid route type '{routeType}' found");
                return null;
            }

            Uri procedureUri = giveMeUri(jsonResponse, "Procedure_x003a_TripleStoreId");
            if (procedureUri != null)
                procedureRoute.ProcedureRouteHasProcedure = new List<IProcedure>()
                {
                    new Procedure()
                    {
                        Id=procedureUri                        
                    }
                };
            else
                return null;
            Uri fromStepUri = giveMeUri(jsonResponse, "FromStep_x003a_TripleStoreId");
            if (fromStepUri != null)
            {
                ProcedureStep fromProcedureStep = new ProcedureStep();
                fromProcedureStep.Id = fromStepUri;
                switch (procedureRouteKind)
                {
                    case ProcedureRouteType.Allows:
                        fromProcedureStep.ProcedureStepAllowsAllowedProcedureRoute = new List<IAllowedProcedureRoute>()
                        {
                            new AllowedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                    case ProcedureRouteType.Causes:
                        fromProcedureStep.ProcedureStepCausesCausedProcedureRoute = new List<ICausedProcedureRoute>()
                        {
                            new CausedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                    case ProcedureRouteType.Precludes:
                        fromProcedureStep.ProcedureStepPrecludesPrecludedProcedureRoute = new List<IPrecludedProcedureRoute>()
                        {
                            new PrecludedProcedureRoute()
                            {
                                Id=procedureRoute.Id
                            }
                        };
                        break;
                }
                procedureRoute.ProcedureRouteIsFromProcedureStep = new List<IProcedureStep>()
                {
                    fromProcedureStep
                };
            }
            else
                return null;
            Uri toStepUri = giveMeUri(jsonResponse, "ToStep_x003a_TripleStoreId");
            if (toStepUri != null)
                procedureRoute.ProcedureRouteIsToProcedureStep = new List<IProcedureStep>()
                {
                    new ProcedureStep()
                    {
                        Id=toStepUri
                    }
                };
            else
                return null;

            return new IResource[] { procedureRoute };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IProcedureRoute>()
                .SingleOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IProcedureRoute procedureRoute = source.OfType<IProcedureRoute>().SingleOrDefault();

            return new IResource[] { procedureRoute };
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            string id = ((JValue)jsonResponse.SelectToken($"{tokenName}.Value")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning($"No {tokenName} Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }
    }
}
