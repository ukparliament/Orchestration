using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationPartyMembershipMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
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
            IMnisPartyMembership mnisPartyMembership = new MnisPartyMembership();
            mnisPartyMembership.PartyMembershipMnisId = partyElement.Element(d + "MemberParty_Id").GetText();

            return new IResource[] { partyMembership, mnisPartyMembership };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string partyMembershipMnisId = deserializedSource.OfType<IMnisPartyMembership>()
                .SingleOrDefault()
                .PartyMembershipMnisId;
            return new Dictionary<string, INode>()
            {
                { "partyMembershipMnisId", SparqlConstructor.GetNode(partyMembershipMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IPartyMembership partyMembership in source.OfType<IPartyMembership>())
                partyMembership.Id = subjectUri;
            return source.OfType<IPartyMembership>().ToArray();
        }

    }
}