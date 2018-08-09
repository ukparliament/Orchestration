﻿using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureBusinessItem
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            BusinessItem businessItem = new BusinessItem();
            Laying laying = new Laying();
            if ((dataset.Tables.Count != 2) ||
                (dataset.Tables[0].Rows.Count != 1) ||
                (dataset.Tables[1].Rows.Count < 1))
                return null;
            DataRow biRow = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(biRow["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
            {
                businessItem.Id = idUri;
                laying.Id = idUri;
            }
            if (Convert.ToBoolean(biRow["IsDeleted"]))
                return new BaseResource[] { businessItem };

            if ((DateTimeOffset.TryParse(biRow["BusinessItemDate"]?.ToString(), out DateTimeOffset dateTime))
                && (dateTime != null))
                businessItem.BusinessItemDate = new DateTimeOffset[] { dateTime };
            Uri workPackageUri = GiveMeUri(GetText(biRow["WorkPackage"]));
            if (workPackageUri != null)
                businessItem.BusinessItemHasWorkPackage = new WorkPackage()
                {
                    Id = workPackageUri
                };
            string url = GetText(biRow["WebLink"]);
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                businessItem.BusinessItemHasBusinessItemWebLink = new List<BusinessItemWebLink>()
                {
                    new BusinessItemWebLink()
                    {
                        Id=uri
                    }
                };
            Uri layingBodyUri = GiveMeUri(GetText(biRow["LayingBody"]));
            if (layingBodyUri != null)
            {
                laying.LayingHasLayingBody = new LayingBody()
                {
                    Id = layingBodyUri
                };
                Uri workPackagedUri = GiveMeUri(GetText(biRow["WorkPackaged"]));
                if (workPackagedUri != null)
                    laying.LayingHasLaidThing = new List<LaidThing>
                    {
                        new LaidThing()
                        {
                            Id = workPackagedUri
                        }
                    };
            }

            businessItem.BusinessItemHasProcedureStep = giveMeUris(dataset.Tables[1], "TripleStoreId")
                .Select(u => new ProcedureStep() { Id = u })
                .ToArray();

            return new BaseResource[] { businessItem, laying };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<BusinessItem>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

        private IEnumerable<Uri> giveMeUris(DataTable dataTable, string columnName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                string itemId = row[columnName]?.ToString();
                if (string.IsNullOrWhiteSpace(itemId))
                    continue;
                if (Uri.TryCreate($"{idNamespace}{itemId}", UriKind.Absolute, out Uri itemUri))
                    yield return itemUri;
            }
        }
    }
}
