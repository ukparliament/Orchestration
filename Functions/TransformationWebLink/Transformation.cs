using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationWebLink
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            PersonWebLink personWebLink = new PersonWebLink();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string url = ((JValue)jsonResponse.SelectToken("URL")).GetText();
            if (string.IsNullOrWhiteSpace(url))
            {
                logger.Warning("No url info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    personWebLink.Id = uri;
                else
                {
                    logger.Warning($"Invalid url '{url}' found");
                    return null;
                }
            }
            int? linkType = ((JValue)jsonResponse.SelectToken("x.Id")).GetInteger();
            if ((linkType != 6) && (linkType != 7) && (linkType != 8))
            {
                logger.Verbose("WebLink information marked as excluded");
                return new BaseResource[] { personWebLink };
            }

            string mnisId = ((JValue)jsonResponse.SelectToken("Person_x003a_MnisId.Value")).GetText();
            if (string.IsNullOrWhiteSpace(mnisId))
            {
                logger.Warning("No member info found");
                return null;
            }
            mnisId = Convert.ToInt32(Convert.ToDouble(mnisId)).ToString();
            Uri personUri = IdRetrieval.GetSubject("memberMnisId", mnisId, false, logger);
            if (personUri != null)
                personWebLink.PersonWebLinkHasPerson = new List<Person>
                {
                    new Person()
                    {
                        Id=personUri
                    }
                };
            else
                logger.Warning("No person found");


            return new BaseResource[] { personWebLink };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<PersonWebLink>()
                .SingleOrDefault()
                .Id;            
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            PersonWebLink personWebLink = source.OfType<PersonWebLink>().SingleOrDefault();

            return new BaseResource[] { personWebLink };
        }
    }
}
