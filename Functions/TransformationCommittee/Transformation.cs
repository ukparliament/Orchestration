using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationCommittee
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
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
                    formalBody.Id = idUri;
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
                        Id = committeeTypeUri
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
                            Id = houseUri
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
                        Id=GenerateNewId(),
                        Email=((JValue)jsonResponse.SelectToken("Email")).GetText(),
                        PhoneNumber=((JValue)jsonResponse.SelectToken("Phone")).GetText()
                    }
                };

            return new IResource[] { formalBody };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IFormalBody>()
                .SingleOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IFormalBody formalBody = source.OfType<IFormalBody>().SingleOrDefault();
            formalBody.Id = subjectUri;

            if (formalBody.FormalBodyHasContactPoint != null)
            {
                IContactPoint contactPoint = target.OfType<IContactPoint>().SingleOrDefault();
                if (contactPoint != null)
                    formalBody.FormalBodyHasContactPoint.SingleOrDefault().Id = contactPoint.Id;
            }

            return source.OfType<IFormalBody>().ToArray();
        }

    }
}
