using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationLordsSeat
{
    public class Transformation : BaseTransformation<Settings>
    {
        
        public override BaseResource[] TransformSource(string response)
        {
            HouseSeat houseSeat = new HouseSeat();
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


            houseSeat.HouseSeatHasHouse = new House()
            {
                Id = new Uri(HouseOfLordsId)
            };

            return new BaseResource[] { houseSeat };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<HouseSeat>()
                .SingleOrDefault()
                .Id;            
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
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
