using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationAnsweringBodyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IMnisAnsweringBody answeringBody = new MnisAnsweringBody();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            answeringBody.AnsweringBodyMnisId = element.Element(d + "AnsweringBody_Id").GetText();
            IMnisDepartmentGroup department = new MnisDepartmentGroup();
            string departmentId = element.Element(d + "Department_Id").GetText();
            if (string.IsNullOrWhiteSpace(departmentId) == false)
            {
                Uri departmentUri = IdRetrieval.GetSubject("mnisDepartmentId", departmentId, false, logger);
                if (departmentUri != null)
                    department.Id = departmentUri;
            }

            return new IResource[] { answeringBody, department };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string answeringBodyMnisId = deserializedSource.OfType<IMnisAnsweringBody>()
                .SingleOrDefault()
                .AnsweringBodyMnisId;
            return new Dictionary<string, object>()
            {
                { "answeringBodyMnisId", answeringBodyMnisId }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IMnisAnsweringBody answeringBody = source.OfType<IMnisAnsweringBody>().SingleOrDefault();
            IMnisDepartmentGroup departmentGroup = source.OfType<IMnisDepartmentGroup>().SingleOrDefault();
            if (departmentGroup != null)
                answeringBody.Id = departmentGroup.Id;
            else
                answeringBody.Id = subjectUri;

            return new IResource[] { answeringBody };
        }

    }
}
