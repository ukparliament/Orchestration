using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Xml.Linq;

namespace Functions.TransformationContactPointPersonMnis
{
    public class Transformation : BaseTransformationContactPoint<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            MnisContactPoint contactPoint = new MnisContactPoint();
            XElement contactPointElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            contactPoint.ContactPointMnisId = contactPointElement.Element(d + "MemberAddress_Id").GetText();
            contactPoint.Email = contactPointElement.Element(d + "Email").GetText();
            contactPoint.FaxNumber = contactPointElement.Element(d + "Fax").GetText();
            contactPoint.PhoneNumber = contactPointElement.Element(d + "Phone").GetText();
            contactPoint.ContactPointHasPostalAddress = GeneratePostalAddress(contactPointElement);
            Person person = generatePerson(contactPointElement);
            if (person != null)
                contactPoint.ContactPointHasPerson = new Person[] { person };

            return new BaseResource[] { contactPoint };
        }

        private Person generatePerson(XElement contactPointElement)
        {
            Person person = null;
            string mnisId = contactPointElement.Element(d + "Member_Id").GetText();
            if (string.IsNullOrWhiteSpace(mnisId))
            {
                logger.Warning("No member info found");
                return null;
            }
            Uri personUri = IdRetrieval.GetSubject("memberMnisId", mnisId, false, logger);
            if (personUri != null)
                person = new Person()
                {
                    Id = personUri
                };
            else
                logger.Verbose("No person found");

            return person;
        }

    }
}
