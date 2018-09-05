using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationGovernmentIncumbencyMnis
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            XElement governmentIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            Incumbency incumbency = new Incumbency();
            incumbency.GovernmentIncumbencyMnisId = governmentIncumbencyElement.Element(d + "MemberGovernmentPost_Id").GetText();
            incumbency.IncumbencyStartDate = governmentIncumbencyElement.Element(d + "StartDate").GetDate();
            string governmentPostMnisId = governmentIncumbencyElement.Element(d + "GovernmentPost_Id").GetText();
            Uri governmentPostUri = IdRetrieval.GetSubject("governmentPositionMnisId", governmentPostMnisId, false, logger);
            if (governmentPostUri == null)
            {
                logger.Warning($"No government position found for {governmentPostMnisId}");
                return null;
            }
            incumbency.GovernmentIncumbencyHasGovernmentPosition = new GovernmentPosition()
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
            incumbency.GovernmentIncumbencyHasGovernmentPerson = new GovernmentPerson()
            {
                Id = memberUri
            };
            incumbency.IncumbencyEndDate = governmentIncumbencyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { incumbency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string governmentIncumbencyMnisId = deserializedSource.OfType<Incumbency>()
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