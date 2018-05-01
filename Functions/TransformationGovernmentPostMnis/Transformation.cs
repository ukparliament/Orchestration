using Parliament.Rdf;
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
        public override IResource[] TransformSource(string response)
        {
            IMnisGovernmentPosition governmentPosition = new MnisGovernmentPosition();
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
            List<IGroup> groups = new List<IGroup>();

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
            return new IResource[] { governmentPosition };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string governmentPositionMnisId = deserializedSource.OfType<IMnisGovernmentPosition>()
                .SingleOrDefault()
                .GovernmentPositionMnisId;
            return new Dictionary<string, INode>()
            {
                { "governmentPositionMnisId", SparqlConstructor.GetNode(governmentPositionMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisGovernmentPosition governmentPosition = source.OfType<IMnisGovernmentPosition>().SingleOrDefault();
            governmentPosition.Id = subjectUri;

            return new IResource[] { governmentPosition };
        }

    }
}
