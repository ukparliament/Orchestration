﻿using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions
{
    public class BaseTransformationContactPoint<T> : BaseTransformation<T>
        where T : ITransformationSettings, new()
    {
        protected XNamespace atom = "http://www.w3.org/2005/Atom";
        protected XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        protected XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IMnisContactPoint contactPoint = source.OfType<IMnisContactPoint>().SingleOrDefault();
            contactPoint.SubjectUri = subjectUri;
            IPostalAddress postalAddress = target.OfType<IPostalAddress>().SingleOrDefault();
            if ((postalAddress != null) && (contactPoint.ContactPointHasPostalAddress != null))
                contactPoint.ContactPointHasPostalAddress.SubjectUri = postalAddress.SubjectUri;

            return new IBaseOntology[] { contactPoint };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string contactPointMnisId = deserializedSource.OfType<IMnisContactPoint>()
                .SingleOrDefault()
                .ContactPointMnisId;
            return new Dictionary<string, object>()
            {
                { "contactPointMnisId", contactPointMnisId }
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
                    SubjectUri = GenerateNewId()
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
                        SubjectUri = GenerateNewId()
                    };
                postalAddress.PostCode = postCode;
            }

            return postalAddress;
        }
    }
}