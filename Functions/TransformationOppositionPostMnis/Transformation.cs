using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationOppositionPostMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            MnisOppositionPosition oppositionPosition = new MnisOppositionPosition();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            oppositionPosition.OppositionPositionMnisId = element.Element(d + "OppositionPost_Id").GetText();
            oppositionPosition.PositionName = element.Element(d + "Name").GetText();

            return new BaseResource[] { oppositionPosition };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string oppositionPositionMnisId = deserializedSource.OfType<MnisOppositionPosition>()
                .SingleOrDefault()
                .OppositionPositionMnisId;
            return new Dictionary<string, INode>()
            {
                { "oppositionPositionMnisId", SparqlConstructor.GetNode(oppositionPositionMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            MnisOppositionPosition oppositionPosition = source.OfType<MnisOppositionPosition>().SingleOrDefault();
            oppositionPosition.Id = subjectUri;

            return new BaseResource[] { oppositionPosition };
        }

    }
}
