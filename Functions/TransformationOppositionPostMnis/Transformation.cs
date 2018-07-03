using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationOppositionPostMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            MnisOppositionPosition oppositionPosition = new MnisOppositionPosition();
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
