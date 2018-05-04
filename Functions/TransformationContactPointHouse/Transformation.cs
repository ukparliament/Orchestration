using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationContactPointHouse
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
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
            
            return new IResource[] { contactPoint };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            Uri contactPointHasHouse = deserializedSource.OfType<IContactPoint>()
                .SingleOrDefault()
                .ContactPointHasHouse
                .SingleOrDefault()
                .Id;
            return new Dictionary<string, INode>()
            {
                { "contactPointHasHouse", SparqlConstructor.GetNode(contactPointHasHouse) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IContactPoint contactPoint = source.OfType<IContactPoint>().SingleOrDefault();
            contactPoint.Id = subjectUri;

            if (contactPoint.ContactPointHasPostalAddress != null)
            {
                IPostalAddress postalAddress = target.OfType<IPostalAddress>().SingleOrDefault();
                if (postalAddress != null)
                    contactPoint.ContactPointHasPostalAddress.Id = postalAddress.Id;
            }

            return new IResource[] { contactPoint };
        }
    }
}
