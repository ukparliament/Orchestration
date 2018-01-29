using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationCommitteeLayMemberMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisFormalBodyLayPerson mnisFormalBodyLayPerson = new MnisFormalBodyLayPerson();
            XDocument doc = XDocument.Parse(response);
            XElement formalBodyLayPersonElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            mnisFormalBodyLayPerson.FormalBodyLayPersonMnisId = formalBodyLayPersonElement.Element(d + "CommitteeLayMember_Id").GetText();
            mnisFormalBodyLayPerson.PersonGivenName = formalBodyLayPersonElement.Element(d + "Forename").GetText();
            mnisFormalBodyLayPerson.PersonOtherNames = formalBodyLayPersonElement.Element(d + "MiddleNames").GetText();
            mnisFormalBodyLayPerson.PersonFamilyName = formalBodyLayPersonElement.Element(d + "Surname").GetText();

            string currentGenderText = formalBodyLayPersonElement.Element(d + "Gender").GetText();
            if (string.IsNullOrWhiteSpace(currentGenderText) == false)
            {
                Uri genderUri = IdRetrieval.GetSubject("genderMnisId", currentGenderText, false, logger);
                if (genderUri != null)
                    mnisFormalBodyLayPerson.PersonHasGenderIdentity = new IGenderIdentity[]
                    {
                        new GenderIdentity()
                        {
                            SubjectUri= GenerateNewId(),
                            GenderIdentityHasGender=new Gender()
                            {
                                SubjectUri= genderUri
                            }
                        }
                    };
            }

            string committeeId = formalBodyLayPersonElement.Element(d + "Committee_Id").GetText();
            if (string.IsNullOrWhiteSpace(committeeId) == false)
            {
                Uri formalBodyUri = IdRetrieval.GetSubject("formalBodyMnisId", committeeId, false, logger);
                if (formalBodyUri != null)
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership = new IFormalBodyMembership[]
                    {
                        new PastFormalBodyMembership()
                        {
                            SubjectUri= GenerateNewId(),
                            FormalBodyMembershipHasFormalBody=new FormalBody()
                            {
                                SubjectUri= formalBodyUri
                            },
                            FormalBodyMembershipStartDate=formalBodyLayPersonElement.Element(d + "StartDate").GetDate(),
                            FormalBodyMembershipEndDate=formalBodyLayPersonElement.Element(d + "EndDate").GetDate()
                        }
                    };
            }

            return new IOntologyInstance[] { mnisFormalBodyLayPerson };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string formalBodyLayPersonMnisId = deserializedSource.OfType<IMnisFormalBodyLayPerson>()
                .SingleOrDefault()
                .FormalBodyLayPersonMnisId;
            return new Dictionary<string, object>()
            {
                { "formalBodyLayPersonMnisId", formalBodyLayPersonMnisId }
            };
        }

        public override Dictionary<string, object> GetKeysForTarget(IOntologyInstance[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<IMnisFormalBodyLayPerson>()
                .SingleOrDefault()
                .PersonHasGenderIdentity
                .SingleOrDefault()
                .GenderIdentityHasGender
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "gender", genderUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMnisFormalBodyLayPerson mnisFormalBodyLayPerson = source.OfType<IMnisFormalBodyLayPerson>().SingleOrDefault();
            mnisFormalBodyLayPerson.SubjectUri = subjectUri;
            if (mnisFormalBodyLayPerson.PersonHasGenderIdentity != null)
            {
                IGenderIdentity genderIdentity = target.OfType<IGenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    mnisFormalBodyLayPerson.PersonHasGenderIdentity.SingleOrDefault().SubjectUri = genderIdentity.SubjectUri;
            }
            if (mnisFormalBodyLayPerson.PersonHasFormalBodyMembership != null)
            {
                IFormalBodyMembership formalBodyMembership = target.OfType<IFormalBodyMembership>().SingleOrDefault();
                if (formalBodyMembership != null)
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership.SingleOrDefault().SubjectUri = formalBodyMembership.SubjectUri;
            }
            return source.OfType<IMnisFormalBodyLayPerson>().ToArray();
        }

    }
}
