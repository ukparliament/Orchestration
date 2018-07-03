using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackageablePreceding
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            WorkPackageableThing workPackageableThing = new WorkPackageableThing();
            DataRow row = dataset.Tables[0].Rows[0];
            Uri idUri = GiveMeUri(GetText(row["FollowingWorkPackageable"]));
            if (idUri == null)
                return null;
            else
                workPackageableThing.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { workPackageableThing };

            Uri precedingUri = GiveMeUri(GetText(row["PrecedingWorkPackageable"]));
            if (precedingUri == null)
                return null;
            else
                workPackageableThing.WorkPackageableThingPrecedesWorkPackageableThing = new WorkPackageableThing[] { new WorkPackageableThing() { Id = precedingUri } };

            return new BaseResource[] { workPackageableThing };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<WorkPackageableThing>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

    }
}
