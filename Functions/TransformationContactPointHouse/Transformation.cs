using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationContactPointHouse
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IContactPoint contactPoint = new ContactPoint();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);
            
            string id = ((JValue)jsonResponse.SelectToken("TripleStoreID")).GetText();
            Uri uri = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out uri))
                    contactPoint.ContactPointHasHouse = new IHouse[]
                    {
                        new House()
                        {
                            SubjectUri = uri
                        }
                    };
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            contactPoint.ContactPointHasPostalAddress = new PostalAddress()
            {
                SubjectUri = GenerateNewId(),
                AddressLine1 = ((JValue)jsonResponse.SelectToken("line1")).GetText(),
                AddressLine2 = ((JValue)jsonResponse.SelectToken("line2")).GetText(),
                AddressLine3 = ((JValue)jsonResponse.SelectToken("line3")).GetText(),
                AddressLine4 = ((JValue)jsonResponse.SelectToken("line4")).GetText(),
                AddressLine5 = ((JValue)jsonResponse.SelectToken("line5")).GetText(),
                PostCode = ((JValue)jsonResponse.SelectToken("postCode")).GetText(),
            };
            
            return new IOntologyInstance[] { contactPoint };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri contactPointHasHouse = deserializedSource.OfType<IContactPoint>()
                .SingleOrDefault()
                .ContactPointHasHouse
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "contactPointHasHouse", contactPointHasHouse }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IContactPoint contactPoint = source.OfType<IContactPoint>().SingleOrDefault();
            contactPoint.SubjectUri = subjectUri;

            if (contactPoint.ContactPointHasPostalAddress != null)
            {
                IPostalAddress postalAddress = target.OfType<IPostalAddress>().SingleOrDefault();
                if (postalAddress != null)
                    contactPoint.ContactPointHasPostalAddress.SubjectUri = postalAddress.SubjectUri;
            }

            return new IOntologyInstance[] { contactPoint };
        }
    }
}
