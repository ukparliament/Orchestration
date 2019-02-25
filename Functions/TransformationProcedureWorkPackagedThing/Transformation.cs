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
            WorkPackagedThing workPackagedThing = new WorkPackagedThing();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
                workPackagedThing.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { workPackagedThing };

            string url = GetText(row["WebLink"]);
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                workPackagedThing.WorkPackagedThingHasWorkPackagedThingWebLink = new List<WorkPackagedThingWebLink>()
                {
                    new WorkPackagedThingWebLink()
                    {
                        Id=uri
                    }
                };

            switch (Convert.ToInt32(row["WorkPackagedType"]))
            {
                case 1:
                    workPackagedThing.StatutoryInstrumentPaperName = GetText(row["WorkPackagedThingName"]);
                    workPackagedThing.StatutoryInstrumentPaperComingIntoForceNote = GetText(row["ComingIntoForceNote"]);
                    if ((DateTimeOffset.TryParse(row["ComingIntoForceDate"]?.ToString(), out DateTimeOffset comingIntoForceDate))
                        && (comingIntoForceDate != null))
                        workPackagedThing.StatutoryInstrumentPaperComingIntoForceDate = comingIntoForceDate;
                    if ((DateTimeOffset.TryParse(row["MadeDate"]?.ToString(), out DateTimeOffset madeDate))
                        && (madeDate != null))
                        workPackagedThing.StatutoryInstrumentPaperMadeDate = madeDate;
                    if (int.TryParse(row["Number"]?.ToString(), out int statutoryInstrumentNumber))
                        workPackagedThing.StatutoryInstrumentPaperNumber = statutoryInstrumentNumber;
                    workPackagedThing.StatutoryInstrumentPaperPrefix = GetText(row["Prefix"]);
                    if (int.TryParse(row["StatutoryInstrumentNumberYear"]?.ToString(), out int statutoryInstrumentNumberYear))
                        workPackagedThing.StatutoryInstrumentPaperYear = statutoryInstrumentNumberYear;
                    break;
                case 2:
                    workPackagedThing.ProposedNegativeStatutoryInstrumentPaperName = GetText(row["WorkPackagedThingName"]);
                    break;
                case 3:
                    workPackagedThing.TreatyName = GetText(row["WorkPackagedThingName"]);
                    workPackagedThing.TreatyComingIntoForceNote = new string[] { GetText(row["ComingIntoForceNote"]) };
                    if ((DateTimeOffset.TryParse(row["ComingIntoForceDate"]?.ToString(), out DateTimeOffset treatyComingIntoForceDate))
                        && (treatyComingIntoForceDate != null))
                        workPackagedThing.TreatyComingIntoForceDate = new DateTimeOffset[] { treatyComingIntoForceDate };
                    if (int.TryParse(row["Number"]?.ToString(), out int treatyNumber))
                        workPackagedThing.TreatyCommandPaperNumber = treatyNumber;
                    workPackagedThing.TreatyCommandPaperPrefix = GetText(row["Prefix"]);
                    Uri govOrgUri = GiveMeUri(GetText(row["LeadGovernmentOrganisationTripleStoreId"]));
                    if (govOrgUri == null)
                        return null;
                    else
                        workPackagedThing.TreatyHasLeadGovernmentOrganisation = new GovernmentOrganisation()
                        {
                            Id = govOrgUri
                        };
                    if ((dataset.Tables.Count == 2) && (dataset.Tables[1].Rows.Count > 0))
                        giveMeMemberships(workPackagedThing, dataset.Tables[1].Rows);

                    break;
            }
            return new BaseResource[] { workPackagedThing };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<WorkPackagedThing>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

        private void giveMeMemberships(WorkPackagedThing workPackagedThing, DataRowCollection memberships)
        {
            List<SeriesMembership> series = new List<SeriesMembership>();
            foreach (DataRow row in memberships)
            {
                Uri seriesUri = GiveMeUri(GetText(row["TripleStoreId"]));
                if (seriesUri == null)
                    continue;
                string citation = GetText(row["Citation"]);
                switch (Convert.ToInt32(row["SeriesMembershipId"]))
                {
                    case 1:
                        workPackagedThing.TreatyHasCountrySeriesMembership = new CountrySeriesMembership()
                        {
                            Id = seriesUri,
                            CountrySeriesItemCitation = citation
                        };
                        break;
                    case 2:
                        workPackagedThing.TreatyHasEuropeanUnionSeriesMembership = new EuropeanUnionSeriesMembership()
                        {
                            Id = seriesUri,
                            EuropeanUnionSeriesItemCitation = citation
                        };
                        break;
                    case 3:
                        workPackagedThing.TreatyHasMiscellaneousSeriesMembership = new MiscellaneousSeriesMembership()
                        {
                            Id = seriesUri,
                            MiscellaneousSeriesItemCitation = citation
                        };
                        break;
                    case 4:
                        workPackagedThing.InForceTreatyHasTreatySeriesMembership = new TreatySeriesMembership()
                        {
                            Id = seriesUri,
                            TreatySeriesItemCitation = citation
                        };
                        break;
                }
            }
            workPackagedThing.TreatyHasSeriesMembership = series;
        }

    }

}