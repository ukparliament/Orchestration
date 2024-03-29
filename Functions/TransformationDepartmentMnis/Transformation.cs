﻿using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationDepartmentMnis
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            Group department = new Group();
            XElement departmentElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            department.GroupName = departmentElement.Element(d + "Name").GetText();
            department.GroupStartDate = departmentElement.Element(d + "StartDate").GetDate();
            department.MnisDepartmentId = departmentElement.Element(d + "Department_Id").GetText();
            department.GroupEndDate = departmentElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { department };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string mnisDepartmentId = deserializedSource.OfType<Group>()
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
