using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions
{
    public class BaseTransformationContactPoint<T> : BaseTransformation<T>
        where T : ITransformationSettings, new()
    {
        protected XNamespace atom = "http://www.w3.org/2005/Atom";
        protected XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        protected XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisContactPoint contactPoint = source.OfType<IMnisContactPoint>().SingleOrDefault();
            contactPoint.Id = subjectUri;
            IPostalAddress postalAddress = target.OfType<IPostalAddress>().SingleOrDefault();
            if ((postalAddress != null) && (contactPoint.ContactPointHasPostalAddress != null))
                contactPoint.ContactPointHasPostalAddress.Id = postalAddress.Id;

            return new IResource[] { contactPoint };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string contactPointMnisId = deserializedSource.OfType<IMnisContactPoint>()
                .SingleOrDefault()
                .ContactPointMnisId;
            return new Dictionary<string, INode>()
            {
                { "contactPointMnisId", SparqlConstructor.GetNode(contactPointMnisId) }
            };
        }

        protected IPostalAddress GeneratePostalAddress(XElement contactPointElement)
        {
            IPostalAddress postalAddress = null;
            List<string> addresses = new List<string>();
            for (var i = 1; i <= 5; i++)
            {
                string address = contactPointElement.Element(d + $"Address{i}").GetText();
                if (string.IsNullOrWhiteSpace(address) == false)
                    addresses.Add(address);
            }
            if (addresses.Any())
            {
                postalAddress = new PostalAddress()
                {
                    Id = GenerateNewId()
                };
                for (var i = 0; i < addresses.Count; i++)
                    switch (i)
                    {
                        case 0:
                            postalAddress.AddressLine1 = addresses[i];
                            break;
                        case 1:
                            postalAddress.AddressLine2 = addresses[i];
                            break;
                        case 2:
                            postalAddress.AddressLine3 = addresses[i];
                            break;
                        case 3:
                            postalAddress.AddressLine4 = addresses[i];
                            break;
                        case 4:
                            postalAddress.AddressLine5 = addresses[i];
                            break;
                    }
            }
            string postCode = contactPointElement.Element(d + "Postcode").GetText();
            if (string.IsNullOrWhiteSpace(postCode) == false)
            {
                if (postalAddress == null)
                    postalAddress = new PostalAddress()
                    {
                        Id = GenerateNewId()
                    };
                postalAddress.PostCode = postCode;
            }

            return postalAddress;
        }
    }
}
