using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationCommitteeLayMemberMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
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
                            Id= GenerateNewId(),
                            GenderIdentityHasGender=new Gender()
                            {
                                Id= genderUri
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
                            Id= GenerateNewId(),
                            FormalBodyMembershipHasFormalBody=new FormalBody()
                            {
                                Id= formalBodyUri
                            },
                            FormalBodyMembershipStartDate=formalBodyLayPersonElement.Element(d + "StartDate").GetDate(),
                            FormalBodyMembershipEndDate=formalBodyLayPersonElement.Element(d + "EndDate").GetDate()
                        }
                    };
            }

            return new IResource[] { mnisFormalBodyLayPerson };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string formalBodyLayPersonMnisId = deserializedSource.OfType<IMnisFormalBodyLayPerson>()
                .SingleOrDefault()
                .FormalBodyLayPersonMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyLayPersonMnisId", SparqlConstructor.GetNode(formalBodyLayPersonMnisId) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(IResource[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<IMnisFormalBodyLayPerson>()
                .SingleOrDefault()
                .PersonHasGenderIdentity
                .SingleOrDefault()
                .GenderIdentityHasGender
                .Id;
            return new Dictionary<string, INode>()
            {
                { "gender", SparqlConstructor.GetNode(genderUri) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisFormalBodyLayPerson mnisFormalBodyLayPerson = source.OfType<IMnisFormalBodyLayPerson>().SingleOrDefault();
            mnisFormalBodyLayPerson.Id = subjectUri;
            if (mnisFormalBodyLayPerson.PersonHasGenderIdentity != null)
            {
                IGenderIdentity genderIdentity = target.OfType<IGenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    mnisFormalBodyLayPerson.PersonHasGenderIdentity.SingleOrDefault().Id = genderIdentity.Id;
            }
            if (mnisFormalBodyLayPerson.PersonHasFormalBodyMembership != null)
            {
                IFormalBodyMembership formalBodyMembership = target.OfType<IFormalBodyMembership>().SingleOrDefault();
                if (formalBodyMembership != null)
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership.SingleOrDefault().Id = formalBodyMembership.Id;
            }
            return source.OfType<IMnisFormalBodyLayPerson>().ToArray();
        }

    }
}
