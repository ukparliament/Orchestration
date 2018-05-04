using Parliament.Rdf;
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
        public override IResource[] TransformSource(string response)
        {
            IMnisOppositionPosition oppositionPosition = new MnisOppositionPosition();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            oppositionPosition.OppositionPositionMnisId = element.Element(d + "OppositionPost_Id").GetText();
            oppositionPosition.PositionName = element.Element(d + "Name").GetText();

            return new IResource[] { oppositionPosition };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string oppositionPositionMnisId = deserializedSource.OfType<IMnisOppositionPosition>()
                .SingleOrDefault()
                .OppositionPositionMnisId;
            return new Dictionary<string, INode>()
            {
                { "oppositionPositionMnisId", SparqlConstructor.GetNode(oppositionPositionMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisOppositionPosition oppositionPosition = source.OfType<IMnisOppositionPosition>().SingleOrDefault();
            oppositionPosition.Id = subjectUri;

            return new IResource[] { oppositionPosition };
        }

    }
}
