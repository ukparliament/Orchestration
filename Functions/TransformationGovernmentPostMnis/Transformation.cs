using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationGovernmentPostMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            MnisGovernmentPosition governmentPosition = new MnisGovernmentPosition();
            XDocument doc = XDocument.Parse(response);
            XNamespace atom = "http://www.w3.org/2005/Atom";
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(d + "GovernmentPost_Id").SingleOrDefault();
            if ((element == null) || (element.Parent==null))
                return null;

            XElement elementPosition = element.Parent;
            
            governmentPosition.GovernmentPositionMnisId = elementPosition.Element(d + "GovernmentPost_Id").GetText();
            governmentPosition.PositionName = elementPosition.Element(d + "Name").GetText();
            List<Group> groups = new List<Group>();

            foreach (XElement elementLoop in doc.Descendants(d + "Department_Id"))
            {
                if ((elementLoop != null) && (elementLoop.Parent != null))
                {
                    XElement departmentElement = elementLoop.Parent;
                    string departmentId = departmentElement.Element(d + "Department_Id").GetText();
                    if (string.IsNullOrWhiteSpace(departmentId) == false)
                    {
                        Uri departmentUri = IdRetrieval.GetSubject("mnisDepartmentId", departmentId, false, logger);
                        if (departmentUri != null)
                            groups.Add(new Group() { Id = departmentUri });                            
                    }
                }
            }
            governmentPosition.PositionHasGroup = groups;
            return new BaseResource[] { governmentPosition };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string governmentPositionMnisId = deserializedSource.OfType<MnisGovernmentPosition>()
                .SingleOrDefault()
                .GovernmentPositionMnisId;
            return new Dictionary<string, INode>()
            {
                { "governmentPositionMnisId", SparqlConstructor.GetNode(governmentPositionMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            MnisGovernmentPosition governmentPosition = source.OfType<MnisGovernmentPosition>().SingleOrDefault();
            governmentPosition.Id = subjectUri;

            return new BaseResource[] { governmentPosition };
        }

    }
}
