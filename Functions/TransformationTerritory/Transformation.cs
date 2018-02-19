using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationTerritory
{
    public class Transformation : BaseTransformation<Settings>
    {

        public override IResource[] TransformSource(string response)
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

            return new IResource[] { territory };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string territoryGovRegisterId = deserializedSource.OfType<IGovRegisterTerritory>()
                .SingleOrDefault()
                .TerritoryGovRegisterId;
            return new Dictionary<string, object>()
            {
                { "territoryGovRegisterId", territoryGovRegisterId }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IGovRegisterTerritory territory = source.OfType<IGovRegisterTerritory>().SingleOrDefault();
            territory.Id = subjectUri;

            return new IResource[] { territory };
        }
    }
}
