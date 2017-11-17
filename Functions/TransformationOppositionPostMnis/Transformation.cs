using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationOppositionPostMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisOppositionPosition oppositionPosition = new MnisOppositionPosition();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            oppositionPosition.OppositionPositionMnisId = element.Element(d + "OppositionPost_Id").GetText();
            oppositionPosition.PositionName = element.Element(d + "Name").GetText();

            return new IOntologyInstance[] { oppositionPosition };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string oppositionPositionMnisId = deserializedSource.OfType<IMnisOppositionPosition>()
                .SingleOrDefault()
                .OppositionPositionMnisId;
            return new Dictionary<string, object>()
            {
                { "oppositionPositionMnisId", oppositionPositionMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMnisOppositionPosition oppositionPosition = source.OfType<IMnisOppositionPosition>().SingleOrDefault();
            oppositionPosition.SubjectUri = subjectUri;

            return new IOntologyInstance[] { oppositionPosition };
        }

    }
}
