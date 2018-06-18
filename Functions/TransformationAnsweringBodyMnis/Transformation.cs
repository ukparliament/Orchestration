using Parliament.Model;
using Parliament.Rdf.Serialization;
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
        public override BaseResource[] TransformSource(string response)
        {
            MnisAnsweringBody answeringBody = new MnisAnsweringBody();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            answeringBody.AnsweringBodyMnisId = element.Element(d + "AnsweringBody_Id").GetText();
            
            string GroupHasNameSparqlCommand = @"                
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
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));

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

            string departmentId = element.Element(d + "Department_Id").GetText();
            if (string.IsNullOrWhiteSpace(departmentId) == false)
            {
                Uri departmentUri = IdRetrieval.GetSubject("mnisDepartmentId", departmentId, false, logger);
                if (departmentUri != null)
                    answeringBody.Id = departmentUri;
                else
                {
                    logger.Warning($"Department ({departmentId}) not found for Answering Body ({answeringBody.AnsweringBodyMnisId})");
                    return null;
                }
            }

            return new BaseResource[] { answeringBody };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string answeringBodyMnisId = deserializedSource.OfType<MnisAnsweringBody>()
                .SingleOrDefault()
                .AnsweringBodyMnisId;
            return new Dictionary<string, INode>()
            {
                { "answeringBodyMnisId", SparqlConstructor.GetNode(answeringBodyMnisId) }
            };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<MnisAnsweringBody>().SingleOrDefault().Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            MnisAnsweringBody answeringBody = source.OfType<MnisAnsweringBody>().SingleOrDefault();
            if (answeringBody.Id == null)
                answeringBody.Id = subjectUri;

            return new BaseResource[] { answeringBody };
        }

    }
}
