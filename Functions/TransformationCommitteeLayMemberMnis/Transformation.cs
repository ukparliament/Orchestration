using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationCommitteeLayMemberMnis
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            Person layPerson = new Person();
            XElement formalBodyLayPersonElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            layPerson.FormalBodyLayPersonMnisId = formalBodyLayPersonElement.Element(d + "CommitteeLayMember_Id").GetText();
            layPerson.PersonGivenName = formalBodyLayPersonElement.Element(d + "Forename").GetText();
            layPerson.PersonOtherNames = formalBodyLayPersonElement.Element(d + "MiddleNames").GetText();
            layPerson.PersonFamilyName = formalBodyLayPersonElement.Element(d + "Surname").GetText();

            string currentGenderText = formalBodyLayPersonElement.Element(d + "Gender").GetText();
            if (string.IsNullOrWhiteSpace(currentGenderText) == false)
            {
                Uri genderUri = IdRetrieval.GetSubject("genderMnisId", currentGenderText, false, logger);
                if (genderUri != null)
                    layPerson.PersonHasGenderIdentity = new GenderIdentity[]
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
                {
                    Uri formalBodymembershipId = GenerateNewId();
                    layPerson.PersonHasFormalBodyMembership = new FormalBodyMembership[]
                    {
                        new FormalBodyMembership()
                        {
                            Id= formalBodymembershipId ,
                            FormalBodyMembershipHasFormalBody=new FormalBody()
                            {
                                Id= formalBodyUri
                            },
                            FormalBodyMembershipStartDate=formalBodyLayPersonElement.Element(d + "StartDate").GetDate(),
                            FormalBodyMembershipEndDate=formalBodyLayPersonElement.Element(d + "EndDate").GetDate()
                        }
                    };
                }
            }

            return new BaseResource[] { layPerson };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string formalBodyLayPersonMnisId = deserializedSource.OfType<Person>()
                .SingleOrDefault()
                .FormalBodyLayPersonMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyLayPersonMnisId", SparqlConstructor.GetNode(formalBodyLayPersonMnisId) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<Person>()
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

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            Person mnisFormalBodyLayPerson = source.OfType<Person>().SingleOrDefault();
            mnisFormalBodyLayPerson.Id = subjectUri;
            if (mnisFormalBodyLayPerson.PersonHasGenderIdentity != null)
            {
                GenderIdentity genderIdentity = target.OfType<GenderIdentity>().SingleOrDefault();
                if (genderIdentity != null)
                    mnisFormalBodyLayPerson.PersonHasGenderIdentity.SingleOrDefault().Id = genderIdentity.Id;
            }
            if (mnisFormalBodyLayPerson.PersonHasFormalBodyMembership != null)
            {
                FormalBodyMembership formalBodyMembership = target.OfType<FormalBodyMembership>().SingleOrDefault();
                if (formalBodyMembership != null)
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership.SingleOrDefault().Id = formalBodyMembership.Id;
            }
            return source;
        }

    }
}
