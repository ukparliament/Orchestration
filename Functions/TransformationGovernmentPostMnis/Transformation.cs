using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationGovernmentPostMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisGovernmentPosition governmentPosition = new MnisGovernmentPosition();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            governmentPosition.GovernmentPositionMnisId = element.Element(d + "GovernmentPost_Id").GetText();
            governmentPosition.PositionName = element.Element(d + "Name").GetText();

            return new IOntologyInstance[] { governmentPosition };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string governmentPositionMnisId = deserializedSource.OfType<IMnisGovernmentPosition>()
                .SingleOrDefault()
                .GovernmentPositionMnisId;
            return new Dictionary<string, object>()
            {
                { "governmentPositionMnisId", governmentPositionMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IMnisGovernmentPosition governmentPosition = source.OfType<IMnisGovernmentPosition>().SingleOrDefault();
            governmentPosition.SubjectUri = subjectUri;

            return new IOntologyInstance[] { governmentPosition };
        }

    }
}
