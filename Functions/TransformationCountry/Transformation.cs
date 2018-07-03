using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationCountry
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {
        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            GovRegisterCountry country = new GovRegisterCountry();
            JValue jValue = (JValue)jsonResponse.First.First.SelectToken("key");
            country.CountryGovRegisterId = jValue.GetText();
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].name");
            country.CountryName = DeserializerHelper.GiveMeSingleTextValue(jValue.GetText());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].official-name");
            country.CountryOfficialName = DeserializerHelper.GiveMeSingleTextValue(jValue.GetText());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].citizen-names");
            country.CountryCitizenNames = DeserializerHelper.GiveMeSingleTextValue(jValue.GetText());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].start-date");
            country.GovRegisterCountryStartDate = DeserializerHelper.GiveMeSingleDateValue(jValue.GetDate());
            jValue = (JValue)jsonResponse.First.First.SelectToken("item[0].end-date");
            country.GovRegisterCountryEndDate = DeserializerHelper.GiveMeSingleDateValue(jValue.GetDate());

            return new BaseResource[] { country };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string countryGovRegisterId = deserializedSource.OfType<GovRegisterCountry>()
                .SingleOrDefault()
                .CountryGovRegisterId;
            return new Dictionary<string, INode>()
            {
                { "countryGovRegisterId", SparqlConstructor.GetNode(countryGovRegisterId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            GovRegisterCountry country = source.OfType<GovRegisterCountry>().SingleOrDefault();
            country.Id = subjectUri;

            return new BaseResource[] { country };
        }
    }
}
