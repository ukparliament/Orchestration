using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationContactPointHouse
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {
        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            ContactPoint contactPoint = new ContactPoint();
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
                    contactPoint.ContactPointHasHouse = new House[]
                    {
                        new House()
                        {
                            Id = uri
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
                Id = GenerateNewId(),
                AddressLine1 = ((JValue)jsonResponse.SelectToken("line1")).GetText(),
                AddressLine2 = ((JValue)jsonResponse.SelectToken("line2")).GetText(),
                AddressLine3 = ((JValue)jsonResponse.SelectToken("line3")).GetText(),
                AddressLine4 = ((JValue)jsonResponse.SelectToken("line4")).GetText(),
                AddressLine5 = ((JValue)jsonResponse.SelectToken("line5")).GetText(),
                PostCode = ((JValue)jsonResponse.SelectToken("postCode")).GetText(),
            };

            return new BaseResource[] { contactPoint };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            Uri contactPointHasHouse = deserializedSource.OfType<ContactPoint>()
                .SingleOrDefault()
                .ContactPointHasHouse
                .SingleOrDefault()
                .Id;
            return new Dictionary<string, INode>()
            {
                { "contactPointHasHouse", SparqlConstructor.GetNode(contactPointHasHouse) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            ContactPoint contactPoint = source.OfType<ContactPoint>().SingleOrDefault();
            contactPoint.Id = subjectUri;

            if (contactPoint.ContactPointHasPostalAddress != null)
            {
                PostalAddress postalAddress = target.OfType<PostalAddress>().SingleOrDefault();
                if (postalAddress != null)
                    contactPoint.ContactPointHasPostalAddress.Id = postalAddress.Id;
            }

            return new BaseResource[] { contactPoint };
        }
    }
}
