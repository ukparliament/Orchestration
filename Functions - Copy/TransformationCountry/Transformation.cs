using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationCountry
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
        {
            IGovRegisterCountry country = new GovRegisterCountry();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

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

            return new IBaseOntology[] { country };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string countryGovRegisterId = deserializedSource.OfType<IGovRegisterCountry>()
                .SingleOrDefault()
                .CountryGovRegisterId;
            return new Dictionary<string, object>()
            {
                { "countryGovRegisterId", countryGovRegisterId }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IGovRegisterCountry country = source.OfType<IGovRegisterCountry>().SingleOrDefault();
            country.SubjectUri = subjectUri;

            return new IBaseOntology[] { country };
        }
    }
}
