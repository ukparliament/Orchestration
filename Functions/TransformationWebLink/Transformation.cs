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
        public override IOntologyInstance[] TransformSource(string response)
        {
            IPersonWebLink personWebLink = new PersonWebLink();
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
                    personWebLink.SubjectUri = uri;
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
                return new IOntologyInstance[] { personWebLink };
            }

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


            return new IOntologyInstance[] { personWebLink };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IPersonWebLink>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IPersonWebLink personWebLink = source.OfType<IPersonWebLink>().SingleOrDefault();

            return new IOntologyInstance[] { personWebLink };
        }
    }
}
