using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackage
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
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
            Uri workPackagedThingUri = GiveMeUri(GetText(row["WorkPackagedThing"]));
            if (workPackagedThingUri != null)
                workPackage.WorkPackageHasWorkPackagedThing = new WorkPackagedThing()
                {
                    Id = workPackagedThingUri
                };


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
