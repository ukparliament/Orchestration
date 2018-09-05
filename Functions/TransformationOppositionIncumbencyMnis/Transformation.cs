using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationOppositionIncumbencyMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            XElement oppositionIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            Incumbency oppositionIncumbency = new Incumbency();
            oppositionIncumbency.OppositionIncumbencyMnisId = oppositionIncumbencyElement.Element(d + "MemberOppositionPost_Id").GetText();
            oppositionIncumbency.IncumbencyStartDate = oppositionIncumbencyElement.Element(d + "StartDate").GetDate();
            string oppositionPositionMnisId = oppositionIncumbencyElement.Element(d + "OppositionPost_Id").GetText();
            Uri oppositionPostUri = IdRetrieval.GetSubject("oppositionPositionMnisId", oppositionPositionMnisId, false, logger);
            if (oppositionPostUri == null)
            {
                logger.Warning($"No opposition position found for {oppositionPositionMnisId}");
                return null;
            }
            oppositionIncumbency.OppositionIncumbencyHasOppositionPosition = new OppositionPosition()
            {
                Id = oppositionPostUri
            };
            string memberId = oppositionIncumbencyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("memberMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            oppositionIncumbency.OppositionIncumbencyHasOppositionPerson = new OppositionPerson()
            {
                Id = memberUri
            };
            oppositionIncumbency.IncumbencyEndDate = oppositionIncumbencyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { oppositionIncumbency,  };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string oppositionIncumbencyMnisId = deserializedSource.OfType<Incumbency>()
                .SingleOrDefault()
                .OppositionIncumbencyMnisId;
            return new Dictionary<string, INode>()
            {
                { "oppositionIncumbencyMnisId", SparqlConstructor.GetNode(oppositionIncumbencyMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource incumbency in source)
                incumbency.Id = subjectUri;
            return source;
        }

    }
}