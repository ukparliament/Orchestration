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
            FormalBodyMembership formalBodyMembership = new FormalBodyMembership();
            XElement formalBodyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            formalBodyMembership.FormalBodyMembershipMnisId = formalBodyElement.Element(d + "MemberCommittee_Id").GetText();
            formalBodyMembership.FormalBodyMembershipStartDate = formalBodyElement.Element(d + "StartDate").GetDate();
            formalBodyMembership.FormalBodyMembershipHasFormalBody = generateFormalBodyMembership(formalBodyElement);
            formalBodyMembership.FormalBodyMembershipEndDate = formalBodyElement.Element(d + "EndDate").GetDate();
            generateMembershipMember(formalBodyMembership, formalBodyElement);

            return new BaseResource[] { formalBodyMembership };
        }

        private void generateMembershipMember(FormalBodyMembership formalBodyMembership,
            XElement formalBodyElement)
        {
            string memberMnisId = formalBodyElement.Element(d + "Member_Id").GetText();
            if (string.IsNullOrWhiteSpace(memberMnisId) == false)
            {
                Uri personUri = IdRetrieval.GetSubject("memberMnisId", memberMnisId, false, logger);
                if (personUri != null)
                {
                    if (formalBodyElement.Element(d + "ExOfficio").GetBoolean() == true)
                        formalBodyMembership.ExOfficioMembershipHasMember = new Member()
                        {
                            Id = personUri
                        };
                    else
                        formalBodyMembership.FormalBodyMembershipHasPerson = new Person()
                        {
                            Id = personUri
                        };
                }
                else
                    logger.Warning($"No member found for id {memberMnisId}");
            }
            else
                logger.Warning("No member data");
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
            string formalBodyMembershipMnisId = deserializedSource.OfType<FormalBodyMembership>()
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
