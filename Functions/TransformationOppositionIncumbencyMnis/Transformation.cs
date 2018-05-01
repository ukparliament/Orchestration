using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationOppositionIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IResource[] TransformSource(string response)
        {
            XDocument doc = XDocument.Parse(response);
            XElement oppositionIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            IMnisOppositionIncumbency oppositionIncumbency = new MnisOppositionIncumbency();
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
            IPastIncumbency incumbency = new PastIncumbency();
            incumbency.Id = oppositionIncumbency.Id;
            incumbency.IncumbencyEndDate = oppositionIncumbencyElement.Element(d + "EndDate").GetDate();

            return new IResource[] { oppositionIncumbency, incumbency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string oppositionIncumbencyMnisId = deserializedSource.OfType<IMnisOppositionIncumbency>()
                .SingleOrDefault()
                .OppositionIncumbencyMnisId;
            return new Dictionary<string, INode>()
            {
                { "oppositionIncumbencyMnisId", SparqlConstructor.GetNode(oppositionIncumbencyMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IIncumbency incumbency in source.OfType<IIncumbency>())
                incumbency.Id = subjectUri;
            return source.OfType<IIncumbency>().ToArray();
        }

    }
}