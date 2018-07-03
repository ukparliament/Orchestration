using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationMemberCommitteeMnis
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            MnisFormalBodyMembership mnisFormalBodyMembership = new MnisFormalBodyMembership();
            XElement formalBodyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            mnisFormalBodyMembership.FormalBodyMembershipMnisId = formalBodyElement.Element(d + "MemberCommittee_Id").GetText();
            mnisFormalBodyMembership.FormalBodyMembershipStartDate = formalBodyElement.Element(d + "StartDate").GetDate();
            mnisFormalBodyMembership.FormalBodyMembershipHasFormalBody = generateFormalBodyMembership(formalBodyElement);
            PastFormalBodyMembership pastFormalBodyMembership = new PastFormalBodyMembership();
            pastFormalBodyMembership.FormalBodyMembershipEndDate = formalBodyElement.Element(d + "EndDate").GetDate();
            BaseResource formalBodyMembership = generateMembershipMember(formalBodyElement);

            return new BaseResource[] { mnisFormalBodyMembership, pastFormalBodyMembership, formalBodyMembership };
        }

        private BaseResource generateMembershipMember(XElement formalBodyElement)
        {
            BaseResource formalBodyMembership = null;
            string memberMnisId = formalBodyElement.Element(d + "Member_Id").GetText();
            if (string.IsNullOrWhiteSpace(memberMnisId) == false)
            {
                Uri personUri = IdRetrieval.GetSubject("memberMnisId", memberMnisId, false, logger);
                if (personUri != null)
                {
                    if (formalBodyElement.Element(d + "ExOfficio").GetBoolean() == true)
                        formalBodyMembership = new ExOfficioMembership()
                        {
                            ExOfficioMembershipHasMember = new Member()
                            {
                                Id = personUri
                            }
                        };
                    else
                        formalBodyMembership = new FormalBodyMembership()
                        {
                            FormalBodyMembershipHasPerson = new Person()
                            {
                                Id = personUri
                            }
                        };
                }
                else
                    logger.Warning($"No member found for id {memberMnisId}");
            }
            else
                logger.Warning("No member data");
            return formalBodyMembership;
        }

        private FormalBody generateFormalBodyMembership(XElement formalBodyElement)
        {
            FormalBody formalBody = null;
            string committeeId = formalBodyElement.Element(d + "Committee_Id").GetText();
            if (string.IsNullOrWhiteSpace(committeeId) == false)
            {
                Uri formalBodyUri = IdRetrieval.GetSubject("formalBodyMnisId", committeeId, false, logger);
                if (formalBodyUri != null)
                    formalBody = new FormalBody()
                    {
                        Id = formalBodyUri
                    };
                else
                    logger.Warning($"No committee found for id {committeeId}");
            }
            else
                logger.Warning("No committee data");
            return formalBody;
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string formalBodyMembershipMnisId = deserializedSource.OfType<MnisFormalBodyMembership>()
                .SingleOrDefault()
                .FormalBodyMembershipMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyMembershipMnisId", SparqlConstructor.GetNode(formalBodyMembershipMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource formalBodyMembership in source)
                formalBodyMembership.Id = subjectUri;
            return source;
        }

    }
}
