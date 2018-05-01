using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

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

            string GroupHasNameSparqlCommand = @"
                prefix parl: <https://id.parliament.uk/schema/>
                construct {
                    ?answeringBody parl:groupName ?name.
                }
                where {
                    ?answeringBody 
                        parl:answeringBodyMnisId @id;
                        parl:groupName ?name;
                    .
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(GroupHasNameSparqlCommand);
            sparql.SetLiteral("id", answeringBody.AnsweringBodyMnisId);

            var groupNameGraph = GraphRetrieval.GetGraph(sparql.ToString(),logger);
            bool groupHasNoName = groupNameGraph.IsEmpty;

            if (groupHasNoName == true)
            {
                answeringBody.GroupName = element.Element(d + "Name").GetText();
            }
            else
            {
                var currentName = groupNameGraph.GetTriplesWithPredicate(new Uri("https://id.parliament.uk/schema/groupName")).FirstOrDefault().Object.ToString();
                answeringBody.GroupName = currentName;
            }

            IMnisDepartmentGroup department = null;
            string departmentId = element.Element(d + "Department_Id").GetText();
            if (string.IsNullOrWhiteSpace(departmentId) == false)
            {
                department = new MnisDepartmentGroup();
                Uri departmentUri = IdRetrieval.GetSubject("mnisDepartmentId", departmentId, false, logger);
                if (departmentUri != null)
                    department.Id = departmentUri;
                else
                {
                    logger.Warning($"Department ({departmentId}) not found for Answering Body ({answeringBody.AnsweringBodyMnisId})");
                    return null;
                }
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
