using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedure
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            Procedure procedure = new Procedure();
            DataRow row = dataset.Tables[0].Rows[0];

            string id = GetText(row["TripleStoreId"]);
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    procedure.Id = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { procedure };
            procedure.ProcedureName = GetText(row["ProcedureName"]);
            procedure.ProcedureDescription = GetText(row["ProcedureDescription"]);

            return new BaseResource[] { procedure };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<Procedure>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
    }
}
