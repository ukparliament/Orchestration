using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace Functions.TransformationProcedureWorkPackage
{
    public class Transformation : BaseTransformationSqlServer<Settings,DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            WorkPackage workPackage = new WorkPackage();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
                workPackage.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { workPackage };
            Uri procedureUri = GiveMeUri(GetText(row["Procedure"]));
            if (procedureUri == null)
                return null;
            else
                workPackage.WorkPackageHasProcedure = new Procedure()
                {
                    Id = procedureUri
                };
            string workPackagableThingType = GetText(row["ProcedureWorkPackageableThingTypeName"]);
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
                Uri workPackageableUri = GiveMeUri(GetText(row["WorkPackageableThing"]));
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

        
    }
}
