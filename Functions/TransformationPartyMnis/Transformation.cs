using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationPartyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisParty party = new MnisParty();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            party.PartyMnisId = element.Element(d + "Party_Id").GetText();
            party.PartyName = element.Element(d + "Name").GetText();

            return new IOntologyInstance[] { party };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string partyMnisId = deserializedSource.OfType<IMnisParty>()
                .SingleOrDefault()
                .PartyMnisId;
            return new Dictionary<string, object>()
            {
                { "partyMnisId", partyMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMnisParty party = source.OfType<IMnisParty>().SingleOrDefault();
            party.SubjectUri = subjectUri;

            return new IOntologyInstance[] { party };
        }
    }
}
