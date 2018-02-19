using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationPartyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IMnisParty party = new MnisParty();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            party.PartyMnisId = element.Element(d + "Party_Id").GetText();
            party.PartyName = element.Element(d + "Name").GetText();

            return new IResource[] { party };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string partyMnisId = deserializedSource.OfType<IMnisParty>()
                .SingleOrDefault()
                .PartyMnisId;
            return new Dictionary<string, object>()
            {
                { "partyMnisId", partyMnisId }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisParty party = source.OfType<IMnisParty>().SingleOrDefault();
            party.Id = subjectUri;

            return new IResource[] { party };
        }
    }
}
