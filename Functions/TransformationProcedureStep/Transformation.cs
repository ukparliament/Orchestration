using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationProcedureStep
{
    public class Transformation : BaseTransformation<Settings>
    {
        private Uri houseUri;

        public override IResource[] TransformSource(string response)
        {
            IProcedureStep procedureStep = new ProcedureStep();
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
            List<IHouse> houses= new List<IHouse>();
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
            return new IResource[] { procedureStep };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IProcedureStep>()
                .SingleOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IProcedureStep procedureStep = source.OfType<IProcedureStep>().SingleOrDefault();

            return new IResource[] { procedureStep };
        }
    }
}
