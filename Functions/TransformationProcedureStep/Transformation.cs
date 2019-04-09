using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureStep
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            ProcedureStep procedureStep = new ProcedureStep();
            DataRow stepRow = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(stepRow["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
                procedureStep.Id = idUri;
            if (Convert.ToBoolean(stepRow["IsDeleted"]))
                return new BaseResource[] { procedureStep };

            procedureStep.ProcedureStepName = GetText(stepRow["ProcedureStepName"]);
            procedureStep.ProcedureStepDescription = GetText(stepRow["ProcedureStepDescription"]);
            var scopeNote = GetText(stepRow["ProcedureStepScopeNote"]);
            if (!String.IsNullOrEmpty(scopeNote))
                procedureStep.ProcedureStepScopeNote = new string[] { scopeNote };
            var linkNote = GetText(stepRow["ProcedureStepLinkNote"]);
            if (!String.IsNullOrEmpty(linkNote))
                procedureStep.ProcedureStepLinkNote = new string[] { linkNote };
            var dateNote = GetText(stepRow["ProcedureStepDateNote"]);
            if (!String.IsNullOrEmpty(dateNote))
                procedureStep.ProcedureStepDateNote = new string[] { dateNote };
            List<House> houses = new List<House>();
            if ((dataset.Tables.Count == 2) && (dataset.Tables[1].Rows != null))
                foreach (DataRow row in dataset.Tables[1].Rows)
                {
                    Uri houseUri = GiveMeUri(GetText(row["House"]));
                    if (houseUri == null)
                        continue;
                    else
                        houses.Add(new House()
                        {
                            Id = houseUri
                        });
                }
            procedureStep.ProcedureStepHasHouse = houses;
            return new BaseResource[] { procedureStep };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<ProcedureStep>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

    }
}
