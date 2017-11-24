using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationHouseSeatType
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisHouseSeatType houseSeatType = new MnisHouseSeatType();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);

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
                    houseSeatType.SubjectUri = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            houseSeatType.HouseSeatTypeName= ((JValue)jsonResponse.SelectToken("Name")).GetText();
            houseSeatType.HouseSeatTypeDescription = ((JValue)jsonResponse.SelectToken("Description")).GetText();

            float? mnisId = ((JValue)jsonResponse.SelectToken("MNIS_x0020_ID")).GetFloat();
            if (mnisId.HasValue)
            {
                houseSeatType.SubjectUri = uri;
                houseSeatType.HouseSeatTypeMnisId = Convert.ToInt32(mnisId).ToString();
            }
            else
                logger.Verbose("No mnis id found");

            return new IOntologyInstance[] { houseSeatType };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IHouseSeatType>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IHouseSeatType houseSeatTypes = source.OfType<IHouseSeatType>().SingleOrDefault();

            return new IOntologyInstance[] { houseSeatTypes };
        }
    }
}
