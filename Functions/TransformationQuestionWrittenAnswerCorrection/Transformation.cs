using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationQuestionWrittenAnswerCorrection
{
    public class Transformation : BaseTransformationXml<Settings,XDocument>
    {
        private string questionUriText;
        public XElement FindXElementByAttributeName(List<XElement> elements, string nameValue, string valueElementName)
        {
            var element = elements.Where(x => x.Attribute("name").Value == nameValue).FirstOrDefault();
            if (element == null)
                return element;
            return element.Element(valueElementName);
        }

        private static Uri GetMemberId(string objectValue, DateTimeOffset? DateTabled, Logger logger)
        {
            string command = @"
                construct{
                    ?member parl:sesId @objectValue.
                }
                where{
                    ?member parl:sesId @objectValue.
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            sparql.SetLiteral("objectValue", objectValue);
            IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "true");
            IEnumerable<INode> nodes = graph.Triples.SubjectNodes;
            Uri result = null;
            if (nodes.Count() > 1)
            {
                command = @"
                construct{
                    ?member parl:sesId @objectValue.
                }
                where{
                    ?member parl:sesId @objectValue;
                            parl:memberHasParliamentaryIncumbency ?incumbency.
                    ?incumbency   parl:startDate  ?startDate.
                    OPTIONAL { ?incumbency    parl:endDate    ?endDate.}
                    FILTER (?startDate <= @dateTabled)
                    FILTER ( !bound(?endDate) || (bound(?endDate) && ?endDate >= @dateTabled ))
                }";
                sparql = new SparqlParameterizedString(command);
                sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                sparql.SetLiteral("objectValue", objectValue);
                sparql.SetParameter("dateTabled", DateTabled.GetValueOrDefault().Date.ToLiteralDate(new NodeFactory()));
                graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "true");
                result = ((IUriNode)graph.Triples.SubjectNodes.SingleOrDefault()).Uri;
            }
            else
                result = ((IUriNode)nodes.SingleOrDefault()).Uri;
            logger.Verbose($"Found existing ({result})");
            return result;
        }

        public override BaseResource[] TransformSource(XDocument doc)
        {
            var questionElements = doc?.Element("response")?.Element("result")?.Element("doc")?.Elements("arr")?.ToList();

            if (questionElements == null)
                return null;
            Response data = new Response();
            data.QuestionUri = FindXElementByAttributeName(questionElements, "correctedItem_uri", "str").GetText();
            if (data.QuestionUri == null)
                data.QuestionUri = FindXElementByAttributeName(questionElements, "correctedItem_t", "str").GetText();
            questionUriText = data.QuestionUri;
            data.CorrectingAnsweringDeptSesId = FindXElementByAttributeName(questionElements, "answeringDept_ses", "int").GetText();
            data.CorrectingAnsweringMemberSesId = FindXElementByAttributeName(questionElements, "correctingMember_ses", "int").GetText();
            data.CorrectingAnswerText = FindXElementByAttributeName(questionElements, "content_t", "str").GetText();

            var dateElements = doc.Element("response").Element("result").Element("doc").Elements("date").ToList();
            data.CorrectingDateOfAnswer = dateElements.Where(x => x.Attribute("name").Value == "date_dt").FirstOrDefault().GetDate();

            CorrectingAnswer correctingAnswer = new CorrectingAnswer();
            correctingAnswer.Id = GenerateNewId();
            correctingAnswer.AnswerText = new string[] { data.CorrectingAnswerText };
            correctingAnswer.AnswerGivenDate = data.CorrectingDateOfAnswer;
            if (string.IsNullOrWhiteSpace(data.CorrectingAnsweringMemberSesId))
                logger.Warning("No information about correcting minister");
            else
            {
                Uri ministerId = GetMemberId(data.CorrectingAnsweringMemberSesId, data.CorrectingDateOfAnswer, logger);

                if (ministerId != null)
                    correctingAnswer.AnswerHasAnsweringPerson = new Person[]
                        {
                        new Person()
                        {
                            Id=ministerId
                        }
                        };
                else
                    logger.Warning($"Minister with Ses Id ({data.CorrectingAnsweringMemberSesId}) not found");
            }
            Uri questionId = IdRetrieval.GetSubject("indexingAndSearchUri", data.QuestionUri, false, logger);

            Question question = null;
            if (questionId != null)
            {
                question = new Question()
                {
                    Id = questionId
                };
                correctingAnswer.CorrectingAnswerHasQuestion = new Question[] { question };
            }
            else
            {
                logger.Warning($"Question with Uri ({data.QuestionUri}) not found");
                return null;
            }

            Answer originalAnswer = giveMeOriginalAnswer(questionId);
            if (originalAnswer != null)
                correctingAnswer.AnswerReplacesAnswer = originalAnswer;
            else
            {
                logger.Warning($"No answer found to replace for question with Uri ({data.QuestionUri})");
                return null;
            }

            AnsweringBodyAllocation correctingAnsweringBodyAllocation = giveMeCorrectingAnsweringBodyAllocation(data, questionId);
            if (correctingAnsweringBodyAllocation != null)
            {
                question.QuestionHasAnsweringBodyAllocation = new AnsweringBodyAllocation[] { correctingAnsweringBodyAllocation };
                correctingAnsweringBodyAllocation.AnsweringBodyAllocationHasAnsweringBody.AnsweringBodyHasWrittenAnswer
                    = new WrittenAnswer[] { new WrittenAnswer() { Id = correctingAnswer.Id } };
            }
            else
                return null;

            IndexingAndSearchThing iast = new IndexingAndSearchThing();
            var uriElements = doc.Element("response").Element("result").Element("doc").Elements("str").ToList();
            iast.IndexingAndSearchUri = new String[] { uriElements.Where(x => x.Attribute("name").Value == "uri").FirstOrDefault().GetText() };

            return new BaseResource[] { correctingAnswer, iast };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string correctingAnswerUri = deserializedSource.OfType<IndexingAndSearchThing>()
                .SingleOrDefault()
                .IndexingAndSearchUri
                .SingleOrDefault();
            return new Dictionary<string, INode>()
            {
                { "correctingAnswerUri", SparqlConstructor.GetNode(correctingAnswerUri) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            Uri id = deserializedSource.OfType<CorrectingAnswer>()
                .SingleOrDefault()
                .CorrectingAnswerHasQuestion
                .SingleOrDefault()
                .Id;
            return new Dictionary<string, INode>()
            {
                { "question", SparqlConstructor.GetNode(id) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            CorrectingAnswer correctingAnswer = source.OfType<CorrectingAnswer>().SingleOrDefault();
            correctingAnswer.Id = subjectUri;
            correctingAnswer.CorrectingAnswerHasQuestion
                .SingleOrDefault()
                .QuestionHasAnsweringBodyAllocation
                .SingleOrDefault()
                .AnsweringBodyAllocationHasAnsweringBody
                .AnsweringBodyHasWrittenAnswer
                .SingleOrDefault()
                .Id = subjectUri;
            IndexingAndSearchThing iast = source.OfType<IndexingAndSearchThing>().SingleOrDefault();
            iast.Id = subjectUri;

            return new BaseResource[] { correctingAnswer, iast };
        }

        private AnsweringBodyAllocation giveMeCorrectingAnsweringBodyAllocation(Response data, Uri questionId)
        {
            AnsweringBodyAllocation answeringBodyAllocation = null;

            if (data.CorrectingAnsweringDeptSesId != null)
            {
                Uri answeringBodyId = IdRetrieval.GetSubject("sesId", data.CorrectingAnsweringDeptSesId, false, logger);
                if (answeringBodyId != null)
                {
                    string command = @"
                        construct{
                            ?question parl:questionHasAnsweringBodyAllocation ?questionHasAnsweringBodyAllocation.
                        }
                        where{
                            bind(@question as ?question)
                            ?question parl:questionHasAnsweringBodyAllocation ?questionHasAnsweringBodyAllocation.
                            ?questionHasAnsweringBodyAllocation parl:answeringBodyAllocationHasAnsweringBody @answeringBody.
                        }";
                    SparqlParameterizedString sparql = new SparqlParameterizedString(command);
                    sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                    sparql.SetUri("question", questionId);
                    sparql.SetUri("answeringBody", answeringBodyId);
                    IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "true");
                    answeringBodyAllocation = new AnsweringBodyAllocation()
                    {
                        Id = graph.IsEmpty ? GenerateNewId() : (graph.Triples.SingleOrDefault().Object as IUriNode).Uri,
                        AnsweringBodyAllocationHasAnsweringBody = new AnsweringBody()
                        {
                            Id = answeringBodyId
                        }
                    };
                }
                else
                    logger.Warning($"Answering body with Ses Id ({data.CorrectingAnsweringDeptSesId}) not found");
            }

            return answeringBodyAllocation;
        }

        private Answer giveMeOriginalAnswer(Uri questionId)
        {
            string command = @"
                construct{
                    ?question parl:questionHasAnswer ?originalAnswer.
                }
                where{
                    bind(@question as ?question)
                    ?originalAnswer parl:answerHasQuestion ?question.
                    optional {?originalAnswer parl:answerReplacesAnswer ?replacedAnswer}
                    filter (bound(?replacedAnswer)=false)
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            sparql.SetUri("question", questionId);
            IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "true");
            if (graph.IsEmpty)
                return null;
            else
                return new Answer()
                {
                    Id = (graph.Triples.SingleOrDefault().Object as IUriNode).Uri
                };
        }
    }
}