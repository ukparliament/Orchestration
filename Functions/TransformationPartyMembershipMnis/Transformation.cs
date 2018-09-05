using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationPartyMembershipMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            PartyMembership partyMembership = new PartyMembership();
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
                Id = partyUri
            };
            string memberId = partyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("memberMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            partyMembership.PartyMembershipHasPartyMember = new PartyMember()
            {
                Id = memberUri
            };
            partyMembership.PartyMembershipStartDate = partyElement.Element(d + "StartDate").GetDate();
            partyMembership.PartyMembershipEndDate = partyElement.Element(d + "EndDate").GetDate();
            partyMembership.PartyMembershipMnisId = partyElement.Element(d + "MemberParty_Id").GetText();

            return new BaseResource[] { partyMembership };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string partyMembershipMnisId = deserializedSource.OfType<PartyMembership>()
                .SingleOrDefault()
                .PartyMembershipMnisId;
            return new Dictionary<string, INode>()
            {
                { "partyMembershipMnisId", SparqlConstructor.GetNode(partyMembershipMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource partyMembership in source)
                partyMembership.Id = subjectUri;
            return source;
        }

    }
}