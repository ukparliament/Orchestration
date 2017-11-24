using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationLordsSeat
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IHouseSeat houseSeat = new HouseSeat();
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
                    houseSeat.SubjectUri = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            houseSeat.HouseSeatName = ((JValue)jsonResponse.SelectToken("Name")).GetText();

            object houseSeatTypeId = ((JValue)jsonResponse.SelectToken("Type_x0020_ID_x0020__x0028_for_x.Value"))?.Value;
            if (houseSeatTypeId == null)
            {
                logger.Warning("No house seat type Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri houseSeatTypeUri))
                    houseSeat.HouseSeatHasHouseSeatType = new HouseSeatType()
                    {
                        SubjectUri = houseSeatTypeUri
                    };
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }

            Uri houseOfLordsUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
            houseSeat.HouseSeatHasHouse = new House()
            {
                SubjectUri = houseOfLordsUri
            };

            return new IOntologyInstance[] { houseSeat };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IHouseSeat>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IHouseSeat houseSeatTypes = source.OfType<IHouseSeat>().SingleOrDefault();

            return new IOntologyInstance[] { houseSeatTypes };
        }
    }
}
