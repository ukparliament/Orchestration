using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationCountry
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
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

            return new IResource[] { country };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string countryGovRegisterId = deserializedSource.OfType<IGovRegisterCountry>()
                .SingleOrDefault()
                .CountryGovRegisterId;
            return new Dictionary<string, INode>()
            {
                { "countryGovRegisterId", SparqlConstructor.GetNode(countryGovRegisterId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IGovRegisterCountry country = source.OfType<IGovRegisterCountry>().SingleOrDefault();
            country.Id = subjectUri;

            return new IResource[] { country };
        }
    }
}
