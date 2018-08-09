using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackagedThing
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            StatutoryInstrumentPaper statutoryInstrument = new StatutoryInstrumentPaper();
            ProposedNegativeStatutoryInstrumentPaper proposedNegativeStatutoryInstrument = new ProposedNegativeStatutoryInstrumentPaper();
            MadeStatutoryInstrumentPaper madeStatutoryInstrument = new MadeStatutoryInstrumentPaper();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
            {
                statutoryInstrument.Id = idUri;
                madeStatutoryInstrument.Id = idUri;
                proposedNegativeStatutoryInstrument.Id = idUri;
            }
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { statutoryInstrument };

            string url = GetText(row["WebLink"]);
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                statutoryInstrument.WorkPackagedThingHasWorkPackagedThingWebLink = new List<WorkPackagedThingWebLink>()
                {
                    new WorkPackagedThingWebLink()
                    {
                        Id=uri
                    }
                };
            if (Convert.ToBoolean(row["IsStatutoryInstrument"]))
            {
                statutoryInstrument.StatutoryInstrumentPaperName = GetText(row["WorkPackagedThingName"]);
                statutoryInstrument.StatutoryInstrumentPaperComingIntoForceNote = GetText(row["ComingIntoForceNote"]);
                if ((DateTimeOffset.TryParse(row["ComingIntoForceDate"]?.ToString(), out DateTimeOffset comingIntoForceDate))
                    && (comingIntoForceDate != null))
                   statutoryInstrument.StatutoryInstrumentPaperComingIntoForceDate = comingIntoForceDate;
                if ((DateTimeOffset.TryParse(row["MadeDate"]?.ToString(), out DateTimeOffset madeDate))
                    && (madeDate != null))
                    madeStatutoryInstrument.StatutoryInstrumentPaperMadeDate = madeDate;
                if (int.TryParse(row["StatutoryInstrumentNumber"]?.ToString(), out int statutoryInstrumentNumber))
                    statutoryInstrument.StatutoryInstrumentPaperNumber = statutoryInstrumentNumber;
                statutoryInstrument.StatutoryInstrumentPaperPrefix = GetText(row["StatutoryInstrumentNumberPrefix"]);
                if (int.TryParse(row["StatutoryInstrumentNumberYear"]?.ToString(), out int statutoryInstrumentNumberYear))
                    statutoryInstrument.StatutoryInstrumentPaperYear = statutoryInstrumentNumberYear;
            }
            else
                proposedNegativeStatutoryInstrument.ProposedNegativeStatutoryInstrumentPaperName= GetText(row["WorkPackagedThingName"]);
            return new BaseResource[] { statutoryInstrument, madeStatutoryInstrument, proposedNegativeStatutoryInstrument  };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<StatutoryInstrumentPaper>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
    }
}
