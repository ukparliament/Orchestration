using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationHouseSeatType
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {
        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            MnisHouseSeatType houseSeatType = new MnisHouseSeatType();
            string id = ((JValue)jsonResponse.SelectToken("ID0")).GetText();
            Uri uri = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out uri))
                    houseSeatType.Id = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            houseSeatType.HouseSeatTypeName = ((JValue)jsonResponse.SelectToken("Name")).GetText();
            houseSeatType.HouseSeatTypeDescription = ((JValue)jsonResponse.SelectToken("Description")).GetText();

            float? mnisId = ((JValue)jsonResponse.SelectToken("MNIS_x0020_ID")).GetFloat();
            if (mnisId.HasValue)
            {
                houseSeatType.Id = uri;
                houseSeatType.HouseSeatTypeMnisId = Convert.ToInt32(mnisId).ToString();
            }
            else
                logger.Verbose("No mnis id found");

            return new BaseResource[] { houseSeatType };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<MnisHouseSeatType>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
    }
}
