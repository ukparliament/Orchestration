using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationWebLink
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
        {
            IPersonWebLink personWebLink = new PersonWebLink();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string mnisId = ((JValue)jsonResponse.SelectToken("Person_x003a_MnisId.Value")).GetText();
            if (string.IsNullOrWhiteSpace(mnisId))
            {
                logger.Warning("No member info found");
                return null;
            }
            mnisId = Convert.ToInt32(Convert.ToDouble(mnisId)).ToString();
            Uri personUri = IdRetrieval.GetSubject("personMnisId", mnisId, false, logger);
            if (personUri != null)
                personWebLink.PersonWebLinkHasPerson = new List<IPerson>
                {
                    new Person()
                    {
                        SubjectUri=personUri
                    }
                };
            else
                logger.Warning("No person found");
            string url = ((JValue)jsonResponse.SelectToken("URL")).GetText();
            if (string.IsNullOrWhiteSpace(url))
            {
                logger.Warning("No url info found");
                return null;
            }
            else
                personWebLink.SubjectUri = new Uri(url);

            return new IBaseOntology[] { personWebLink };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IPersonWebLink>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IPersonWebLink personWebLink = source.OfType<IPersonWebLink>().SingleOrDefault();

            return new IBaseOntology[] { personWebLink };
        }
    }
}
