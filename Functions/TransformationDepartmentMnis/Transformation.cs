using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationDepartmentMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override BaseResource[] TransformSource(string response)
        {
            MnisDepartmentGroup department = new MnisDepartmentGroup();

            XDocument doc = XDocument.Parse(response);
            XElement departmentElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            department.GroupName = departmentElement.Element(d + "Name").GetText();
            department.GroupStartDate = departmentElement.Element(d + "StartDate").GetDate();
            department.MnisDepartmentId = departmentElement.Element(d + "Department_Id").GetText();
            PastGroup pastGroup = new PastGroup();
            pastGroup.GroupEndDate = departmentElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { department, pastGroup };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string mnisDepartmentId = deserializedSource.OfType<MnisDepartmentGroup>()
                .SingleOrDefault()
                .MnisDepartmentId;
            return new Dictionary<string, INode>()
            {
                { "mnisDepartmentId", SparqlConstructor.GetNode(mnisDepartmentId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource group in source)
                group.Id = subjectUri;

            return source;
        }
    }
}
