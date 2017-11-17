using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationOppositionIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
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
                SubjectUri = oppositionPostUri
            };
            string memberId = oppositionIncumbencyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("personMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            oppositionIncumbency.OppositionIncumbencyHasOppositionPerson = new OppositionPerson()
            {
                SubjectUri = memberUri
            };
            IPastIncumbency incumbency = new PastIncumbency();
            incumbency.SubjectUri = oppositionIncumbency.SubjectUri;
            incumbency.IncumbencyEndDate = oppositionIncumbencyElement.Element(d + "EndDate").GetDate();

            return new IOntologyInstance[] { oppositionIncumbency, incumbency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string oppositionIncumbencyMnisId = deserializedSource.OfType<IMnisOppositionIncumbency>()
                .SingleOrDefault()
                .OppositionIncumbencyMnisId;
            return new Dictionary<string, object>()
            {
                { "oppositionIncumbencyMnisId", oppositionIncumbencyMnisId }
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