using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationPartyMembershipMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            IPastPartyMembership partyMembership = new PastPartyMembership();
            XDocument doc = XDocument.Parse(response);
            XElement partyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");
            
            string partyId = partyElement.Element(d + "Party_Id").GetText();
            Uri partyUri = IdRetrieval.GetSubject("partyMnisId", partyId, false, logger);
            if (partyUri == null)
            {
                logger.Warning($"No party found for {partyId}");
                return null;
            }
            partyMembership.PartyMembershipHasParty = new Party()
            {
                SubjectUri = partyUri
            };
            string memberId = partyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("personMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            partyMembership.PartyMembershipHasPartyMember = new PartyMember()
            {
                SubjectUri = memberUri
            };
            partyMembership.PartyMembershipStartDate = partyElement.Element(d + "StartDate").GetDate();
            partyMembership.PartyMembershipEndDate = partyElement.Element(d + "EndDate").GetDate();
            IMnisPartyMembership mnisPartyMembership = new MnisPartyMembership();
            mnisPartyMembership.PartyMembershipMnisId = partyElement.Element(d + "MemberParty_Id").GetText();

            return new IOntologyInstance[] { partyMembership, mnisPartyMembership };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string partyMembershipMnisId = deserializedSource.OfType<IMnisPartyMembership>()
                .SingleOrDefault()
                .PartyMembershipMnisId;
            return new Dictionary<string, object>()
            {
                { "partyMembershipMnisId", partyMembershipMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IPartyMembership partyMembership in source.OfType<IPartyMembership>())
                partyMembership.SubjectUri = subjectUri;
            return source.OfType<IPartyMembership>().ToArray();
        }

    }
}