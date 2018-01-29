using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationMemberCommitteeMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
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

            return new IOntologyInstance[] { mnisFormalBodyMembership, pastFormalBodyMembership, formalBodyMembership };
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
                                SubjectUri = personUri
                            }
                        };
                    else
                        formalBodyMembership = new FormalBodyMembership()
                        {
                            FormalBodyMembershipHasPerson = new Member()
                            {
                                SubjectUri = personUri
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
                        SubjectUri = formalBodyUri
                    };
                else
                    logger.Warning($"No committee found for id {committeeId}");
            }
            else
                logger.Warning("No committee data");
            return formalBody;
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string formalBodyMembershipMnisId = deserializedSource.OfType<IMnisFormalBodyMembership>()
                .SingleOrDefault()
                .FormalBodyMembershipMnisId;
            return new Dictionary<string, object>()
            {
                { "formalBodyMembershipMnisId", formalBodyMembershipMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IFormalBodyMembership formalBodyMembership in source.OfType<IFormalBodyMembership>())
                formalBodyMembership.SubjectUri = subjectUri;
            return source.OfType<IFormalBodyMembership>().ToArray();
        }

    }
}
