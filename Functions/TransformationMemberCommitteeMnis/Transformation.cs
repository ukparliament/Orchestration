using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationMemberCommitteeMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
        {
            IMnisFormalBodyMembership mnisFormalBodyMembership = new MnisFormalBodyMembership();
            XDocument doc = XDocument.Parse(response);
            XElement formalBodyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            mnisFormalBodyMembership.FormalBodyMembershipMnisId = formalBodyElement.Element(d + "MemberCommittee_Id").GetText();
            mnisFormalBodyMembership.FormalBodyMembershipStartDate = formalBodyElement.Element(d + "StartDate").GetDate();
            mnisFormalBodyMembership.FormalBodyMembershipHasFormalBody = generateFormalBodyMembership(formalBodyElement);
            IPastFormalBodyMembership pastFormalBodyMembership = new PastFormalBodyMembership();
            pastFormalBodyMembership.FormalBodyMembershipEndDate = formalBodyElement.Element(d + "EndDate").GetDate();
            IFormalBodyMembership formalBodyMembership = generateMembershipMember(formalBodyElement);

            return new IResource[] { mnisFormalBodyMembership, pastFormalBodyMembership, formalBodyMembership };
        }

        private IFormalBodyMembership generateMembershipMember(XElement formalBodyElement)
        {
            IFormalBodyMembership formalBodyMembership = null;
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
                            FormalBodyMembershipHasPerson = new Member()
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

        private IFormalBody generateFormalBodyMembership(XElement formalBodyElement)
        {
            IFormalBody formalBody = null;
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

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string formalBodyMembershipMnisId = deserializedSource.OfType<IMnisFormalBodyMembership>()
                .SingleOrDefault()
                .FormalBodyMembershipMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyMembershipMnisId", SparqlConstructor.GetNode(formalBodyMembershipMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IFormalBodyMembership formalBodyMembership in source.OfType<IFormalBodyMembership>())
                formalBodyMembership.Id = subjectUri;
            return source.OfType<IFormalBodyMembership>().ToArray();
        }

    }
}
