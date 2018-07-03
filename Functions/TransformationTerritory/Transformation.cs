using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationTerritory
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {

        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            GovRegisterTerritory territory = new GovRegisterTerritory();
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

            return new BaseResource[] { territory };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string territoryGovRegisterId = deserializedSource.OfType<GovRegisterTerritory>()
                .SingleOrDefault()
                .TerritoryGovRegisterId;
            return new Dictionary<string, INode>()
            {
                { "territoryGovRegisterId", SparqlConstructor.GetNode(territoryGovRegisterId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            GovRegisterTerritory territory = source.OfType<GovRegisterTerritory>().SingleOrDefault();
            territory.Id = subjectUri;

            return new BaseResource[] { territory };
        }
    }
}
