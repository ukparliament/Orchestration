using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationProcedureStep
{
    public class Transformation : BaseTransformation<Settings>
    {
        private Uri houseUri;

        public override BaseResource[] TransformSource(string response)
        {
            ProcedureStep procedureStep = new ProcedureStep();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("TripleStoreId")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    procedureStep.Id = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            procedureStep.ProcedureStepName = ((JValue)jsonResponse.SelectToken("Title")).GetText();
            procedureStep.ProcedureStepDescription = ((JValue)jsonResponse.SelectToken("Description")).GetText();
            List<House> houses= new List<House>();
            foreach (JObject house in (JArray)jsonResponse.SelectToken("House_x003a_TripleStoreId"))
            {
                string houseId = house["Value"].ToString();
                if (string.IsNullOrWhiteSpace(houseId))
                    continue;
                if (Uri.TryCreate($"{idNamespace}{houseId}", UriKind.Absolute, out houseUri))
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
