using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Functions.TransformationProcedureLaying
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            Laying laying = new Laying();
            DataRow row = dataset.Tables[0].Rows[0];

            Uri idUri = GiveMeUri(GetText(row["TripleStoreId"]));
            if (idUri == null)
                return null;
            else
                laying.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { laying };

            Uri workPackagedUri = GiveMeUri(GetText(row["WorkPackaged"]));
            if (workPackagedUri != null)
                laying.LayingHasLaidThing = new List<LaidThing>
                    {
                        new LaidThing()
                        {
                            Id = workPackagedUri
                        }
                    };
            Uri layingBodyUri = GiveMeUri(GetText(row["LayingBody"]));
            if (layingBodyUri != null)
            {
                laying.LayingHasLayingBody = new LayingBody()
                {
                    Id = layingBodyUri
                };                
            }

            if ((DateTimeOffset.TryParse(row["LayingDate"]?.ToString(), out DateTimeOffset dateTime))
                && (dateTime != null))
                laying.LayingDate = dateTime;
            
            return new BaseResource[] { laying };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<Laying>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
        
    }
}
