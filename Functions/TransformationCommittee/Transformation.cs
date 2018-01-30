using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationCommittee
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IFormalBody formalBody = new FormalBody();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("ResourceUri")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    formalBody.SubjectUri = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }

            List<IFormalBodyType> formalBodyTypes = new List<IFormalBodyType>();
            Uri committeeTypeUri;
            foreach (JObject commiteeType in (JArray)jsonResponse.SelectToken("CommitteeType_x003a_TripleStoreI"))
            {
                string committeeTypeId = commiteeType["Value"].ToString();
                if (string.IsNullOrWhiteSpace(committeeTypeId))
                    continue;
                if (Uri.TryCreate($"{idNamespace}{committeeTypeId}", UriKind.Absolute, out committeeTypeUri))
                    formalBodyTypes.Add(new FormalBodyType()
                    {
                        SubjectUri = committeeTypeUri
                    });
            }
            formalBody.FormalBodyHasFormalBodyType = formalBodyTypes;

            bool? isCommons = ((JValue)jsonResponse.SelectToken("IsCommons")).GetBoolean();
            bool? isLords = ((JValue)jsonResponse.SelectToken("IsLords")).GetBoolean();
            if ((isCommons == true) && (isLords == true))
            {
                string houseId = ((JValue)jsonResponse.SelectToken("LeadHouse_x003a_TripleStoreId.Value")).GetText();
                if (string.IsNullOrWhiteSpace(houseId) == false)
                {
                    if (Uri.TryCreate($"{idNamespace}{houseId}", UriKind.Absolute, out Uri houseUri))
                        formalBody.FormalBodyHasLeadHouse = new House()
                        {
                            SubjectUri=houseUri
                        };
                    else
                    {
                        logger.Warning($"Invalid url '{houseId}' found");
                        return null;
                    }
                }
            }
            formalBody.FormalBodyRemit = ((JValue)jsonResponse.SelectToken("Description")).GetText();
            formalBody.FormalBodyHasContactPoint = new IContactPoint[]
                {
                    new ContactPoint()
                    {
                        SubjectUri=GenerateNewId(),
                        Email=((JValue)jsonResponse.SelectToken("Email")).GetText(),
                        PhoneNumber=((JValue)jsonResponse.SelectToken("Phone")).GetText()
                    }
                };

            return new IOntologyInstance[] { formalBody };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IFormalBody>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IFormalBody formalBody = source.OfType<IFormalBody>().SingleOrDefault();
            formalBody.SubjectUri = subjectUri;

            if (formalBody.FormalBodyHasContactPoint != null)
            {
                IContactPoint contactPoint = target.OfType<IContactPoint>().SingleOrDefault();
                if (contactPoint != null)
                    formalBody.FormalBodyHasContactPoint.SingleOrDefault().SubjectUri = contactPoint.SubjectUri;
            }

            return source.OfType<IFormalBody>().ToArray();
        }

    }
}
