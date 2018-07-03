using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackageableThing
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            WorkPackageableThing workPackageableThing = new WorkPackageableThing();
            StatutoryInstrument statutoryInstrument = new StatutoryInstrument();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
            {
                workPackageableThing.Id = idUri;
                statutoryInstrument.Id = idUri;
            }
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { new WorkPackageableThing() { Id = idUri } };

            workPackageableThing.WorkPackageableThingName = GetText(row["ProcedureWorkPackageableThingName"]);
            workPackageableThing.WorkPackageableThingComingIntoForceNote = GetText(row["ComingIntoForceNote"]);
            if ((DateTimeOffset.TryParse(row["ComingIntoForceDate"]?.ToString(), out DateTimeOffset comingIntoForceDate))
                && (comingIntoForceDate != null))
                workPackageableThing.WorkPackageableThingComingIntoForceDate = comingIntoForceDate;
            if ((DateTimeOffset.TryParse(row["TimeLimitForObjectionEndDate"]?.ToString(), out DateTimeOffset timeLimitForObjectionEndDate))
                && (timeLimitForObjectionEndDate != null))
                workPackageableThing.WorkPackageableThingTimeLimitForObjectionEndDate = timeLimitForObjectionEndDate;
            string url = GetText(row["WebLink"]);
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                workPackageableThing.WorkPackageableThingHasWorkPackageableThingWebLink = new List<WorkPackageableThingWebLink>()
                {
                    new WorkPackageableThingWebLink()
                    {
                        Id=uri
                    }
                };

            statutoryInstrument.StatutoryInstrumentNumber = GetText(row["StatutoryInstrumentNumber"]);
            statutoryInstrument.StatutoryInstrumentNumberPrefix = GetText(row["StatutoryInstrumentNumberPrefix"]);
            if (int.TryParse(row["StatutoryInstrumentNumberYear"]?.ToString(), out int statutoryInstrumentNumberYear))
                statutoryInstrument.StatutoryInstrumentNumberYear = statutoryInstrumentNumberYear;

            return new BaseResource[] { workPackageableThing, statutoryInstrument };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<WorkPackageableThing>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
    }
}
