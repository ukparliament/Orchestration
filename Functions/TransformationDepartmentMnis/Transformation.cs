using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationDepartmentMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            IMnisDepartmentGroup department = new MnisDepartmentGroup();

            XDocument doc = XDocument.Parse(response);
            XElement departmentElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            department.GroupName = departmentElement.Element(d + "Name").GetText();
            department.GroupStartDate = departmentElement.Element(d + "StartDate").GetDate();
            department.MnisDepartmentId = departmentElement.Element(d + "Department_Id").GetText();
            IPastGroup pastGroup = new PastGroup();
            pastGroup.GroupEndDate = departmentElement.Element(d + "EndDate").GetDate();

            return new IOntologyInstance[] { department, pastGroup };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string mnisDepartmentId = deserializedSource.OfType<IMnisDepartmentGroup>()
                .SingleOrDefault()
                .MnisDepartmentId;
            return new Dictionary<string, object>()
            {
                { "mnisDepartmentId", mnisDepartmentId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IGroup group in source.OfType<IGroup>())
                group.SubjectUri = subjectUri;

            IOntologyInstance[] groups = source.OfType<IGroup>().ToArray();
            return groups.ToArray();
        }
    }
}
