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
            MnisFormalBodyLayPerson mnisFormalBodyLayPerson = new MnisFormalBodyLayPerson();
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
                    mnisFormalBodyLayPerson.PersonHasGenderIdentity = new GenderIdentity[]
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
            PastFormalBodyMembership pastFormalBodyMembership = null;
            string committeeId = formalBodyLayPersonElement.Element(d + "Committee_Id").GetText();
            if (string.IsNullOrWhiteSpace(committeeId) == false)
            {
                Uri formalBodyUri = IdRetrieval.GetSubject("formalBodyMnisId", committeeId, false, logger);
                if (formalBodyUri != null)
                {
                    Uri formalBodymembershipId = GenerateNewId();
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership = new FormalBodyMembership[]
                    {
                        new FormalBodyMembership()
                        {
                            Id= formalBodymembershipId ,
                            FormalBodyMembershipHasFormalBody=new FormalBody()
                            {
                                Id= formalBodyUri
                            },
                            FormalBodyMembershipStartDate=formalBodyLayPersonElement.Element(d + "StartDate").GetDate()
                        }
                    };
                    pastFormalBodyMembership = new PastFormalBodyMembership()
                    {
                        Id = formalBodymembershipId,
                        FormalBodyMembershipEndDate = formalBodyLayPersonElement.Element(d + "EndDate").GetDate()
                    };
                }
            }

            return new BaseResource[] { mnisFormalBodyLayPerson, pastFormalBodyMembership };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string formalBodyLayPersonMnisId = deserializedSource.OfType<MnisFormalBodyLayPerson>()
                .SingleOrDefault()
                .FormalBodyLayPersonMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyLayPersonMnisId", SparqlConstructor.GetNode(formalBodyLayPersonMnisId) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            Uri genderUri = deserializedSource.OfType<MnisFormalBodyLayPerson>()
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
            MnisFormalBodyLayPerson mnisFormalBodyLayPerson = source.OfType<MnisFormalBodyLayPerson>().SingleOrDefault();
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
                {
                    mnisFormalBodyLayPerson.PersonHasFormalBodyMembership.SingleOrDefault().Id = formalBodyMembership.Id;
                    PastFormalBodyMembership pastFormalBodyMembership = source.OfType<PastFormalBodyMembership>().SingleOrDefault();
                    if (pastFormalBodyMembership != null)
                        pastFormalBodyMembership.Id = formalBodyMembership.Id;
                }
            }
            return source;
        }

    }
}
