using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationPartyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            MnisParty party = new MnisParty();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
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
