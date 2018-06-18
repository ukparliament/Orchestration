using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Functions.TransformationProcedureWorkPackage
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            WorkPackage workPackage = new WorkPackage();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            Uri idUri = giveMeUri(jsonResponse, "OData__x0076_dy9");
            if (idUri == null)
                return null;
            else
                workPackage.Id = idUri;
            Uri procedureUri = giveMeUri(jsonResponse, "SubjectTo_x003a_TripleStoreId.Value");
            if (procedureUri == null)
                return null;
            else
                workPackage.WorkPackageHasProcedure = new Procedure()
                {
                    Id = procedureUri
                };
            string workPackagableThingType = ((JValue)((JObject)jsonResponse.SelectToken("WorkPackagableThingType"))?.SelectToken("Value"))?.Value?.ToString();
            Dictionary<string, string> types = typeof(ProcedureWorkPackageableThingType)
                .GetFields()
                .Where(f => f.IsLiteral)
                .Select(f => new KeyValuePair<string, string>((f.GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault() as EnumMemberAttribute).Value,
                    f.Name))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            if ((string.IsNullOrWhiteSpace(workPackagableThingType) == false) &&
                (types.ContainsKey(workPackagableThingType)) &&
                (Enum.TryParse<ProcedureWorkPackageableThingType>(types[workPackagableThingType], out ProcedureWorkPackageableThingType workPackagableThingKind)))
            {
                Uri workPackageableUri = giveMeUri(jsonResponse, "WorkPacakageableThingTripleStore");
                if (workPackageableUri != null)
                    switch (workPackagableThingKind)
                    {
                        case ProcedureWorkPackageableThingType.ProposedStatutoryInstrument:
                            workPackage.WorkPackageHasProposedStatutoryInstrument = new ProposedStatutoryInstrument[] { new ProposedStatutoryInstrument() { Id = workPackageableUri } };
                            break;
                        case ProcedureWorkPackageableThingType.StatutoryInstrument:
                            workPackage.WorkPackageHasStatutoryInstrument = new StatutoryInstrument[] { new StatutoryInstrument() { Id = workPackageableUri } };
                            break;
                    }
            }

            return new BaseResource[] { workPackage };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<WorkPackage>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            string id = ((JValue)jsonResponse.SelectToken(tokenName)).GetText();
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
