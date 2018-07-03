using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationGovernmentIncumbencyMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            XElement governmentIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            MnisGovernmentIncumbency governmentIncumbency = new MnisGovernmentIncumbency();
            governmentIncumbency.GovernmentIncumbencyMnisId = governmentIncumbencyElement.Element(d + "MemberGovernmentPost_Id").GetText();
            governmentIncumbency.IncumbencyStartDate = governmentIncumbencyElement.Element(d + "StartDate").GetDate();
            string governmentPostMnisId = governmentIncumbencyElement.Element(d + "GovernmentPost_Id").GetText();
            Uri governmentPostUri = IdRetrieval.GetSubject("governmentPositionMnisId", governmentPostMnisId, false, logger);
            if (governmentPostUri == null)
            {
                logger.Warning($"No government position found for {governmentPostMnisId}");
                return null;
            }
            governmentIncumbency.GovernmentIncumbencyHasGovernmentPosition = new GovernmentPosition()
            {
                Id = governmentPostUri
            };
            string memberId = governmentIncumbencyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("memberMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            governmentIncumbency.GovernmentIncumbencyHasGovernmentPerson = new GovernmentPerson()
            {
                Id = memberUri
            };
            PastIncumbency incumbency = new PastIncumbency();
            incumbency.Id = governmentIncumbency.Id;
            incumbency.IncumbencyEndDate = governmentIncumbencyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { governmentIncumbency, incumbency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string governmentIncumbencyMnisId = deserializedSource.OfType<MnisGovernmentIncumbency>()
                .SingleOrDefault()
                .GovernmentIncumbencyMnisId;
            return new Dictionary<string, INode>()
            {
                { "governmentIncumbencyMnisId", SparqlConstructor.GetNode(governmentIncumbencyMnisId) }
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