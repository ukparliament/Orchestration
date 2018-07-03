using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationPartyMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            MnisParty party = new MnisParty();
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            party.PartyMnisId = element.Element(d + "Party_Id").GetText();
            party.PartyName = element.Element(d + "Name").GetText();

            return new BaseResource[] { party };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string partyMnisId = deserializedSource.OfType<MnisParty>()
                .SingleOrDefault()
                .PartyMnisId;
            return new Dictionary<string, INode>()
            {
                { "partyMnisId", SparqlConstructor.GetNode(partyMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            MnisParty party = source.OfType<MnisParty>().SingleOrDefault();
            party.Id = subjectUri;

            return new BaseResource[] { party };
        }
    }
}
