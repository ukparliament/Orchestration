using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationGovernmentIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            XDocument doc = XDocument.Parse(response);
            XElement governmentIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            IMnisGovernmentIncumbency governmentIncumbency = new MnisGovernmentIncumbency();
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
                SubjectUri = governmentPostUri
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
                SubjectUri = memberUri
            };
            IPastIncumbency incumbency = new PastIncumbency();
            incumbency.SubjectUri = governmentIncumbency.SubjectUri;
            incumbency.IncumbencyEndDate = governmentIncumbencyElement.Element(d + "EndDate").GetDate();

            return new IOntologyInstance[] { governmentIncumbency, incumbency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string governmentIncumbencyMnisId = deserializedSource.OfType<IMnisGovernmentIncumbency>()
                .SingleOrDefault()
                .GovernmentIncumbencyMnisId;
            return new Dictionary<string, object>()
            {
                { "governmentIncumbencyMnisId", governmentIncumbencyMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IIncumbency incumbency in source.OfType<IIncumbency>())
                incumbency.SubjectUri = subjectUri;
            return source.OfType<IIncumbency>().ToArray();
        }

    }
}