using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationLordsSeat
{
    public class Transformation : BaseTransformation<Settings>
    {
        
        public override IResource[] TransformSource(string response)
        {
            IHouseSeat houseSeat = new HouseSeat();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

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
                    houseSeat.Id = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            houseSeat.HouseSeatName = ((JValue)jsonResponse.SelectToken("Name")).GetText();

            Uri houseSeatTypeUri = giveMeUri(jsonResponse, "Type_x0020_ID_x0020__x0028_for_x");
            if (houseSeatTypeUri == null)
                return null;
            else
                houseSeat.HouseSeatHasHouseSeatType = new HouseSeatType()
                {
                    Id = houseSeatTypeUri
                };


            Uri houseOfLordsUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
            houseSeat.HouseSeatHasHouse = new House()
            {
                Id = houseOfLordsUri
            };

            return new IResource[] { houseSeat };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IHouseSeat>()
                .SingleOrDefault()
                .Id;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IHouseSeat houseSeatTypes = source.OfType<IHouseSeat>().SingleOrDefault();

            return new IResource[] { houseSeatTypes };
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            object id = ((JValue)jsonResponse.SelectToken($"{tokenName}.Value"))?.Value;
            if (id == null)
            {
                logger.Warning($"No {tokenName} Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }
    }
}
