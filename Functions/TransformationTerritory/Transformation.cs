using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationTerritory
{
    public class Transformation : BaseTransformation<Settings>
    {

        public override IOntologyInstance[] TransformSource(string response)
        {
            IGovRegisterTerritory territory = new GovRegisterTerritory();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            JValue jValue = (JValue)jsonResponse.First.First.SelectToken("key");
            territory.TerritoryGovRegisterId = jValue.GetText();
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].name");
            territory.TerritoryName = DeserializerHelper.GiveMeSingleTextValue(jValue.GetText());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].official-name");
            territory.TerritoryOfficialName = DeserializerHelper.GiveMeSingleTextValue(jValue.GetText());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].start-date");
            territory.GovRegisterTerritoryStartDate = DeserializerHelper.GiveMeSingleDateValue(jValue.GetDate());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].end-date");
            territory.GovRegisterTerritoryEndDate = DeserializerHelper.GiveMeSingleDateValue(jValue.GetDate());

            return new IOntologyInstance[] { territory };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string territoryGovRegisterId = deserializedSource.OfType<IGovRegisterTerritory>()
                .SingleOrDefault()
                .TerritoryGovRegisterId;
            return new Dictionary<string, object>()
            {
                { "territoryGovRegisterId", territoryGovRegisterId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IGovRegisterTerritory territory = source.OfType<IGovRegisterTerritory>().SingleOrDefault();
            territory.SubjectUri = subjectUri;

            return new IOntologyInstance[] { territory };
        }
    }
}
