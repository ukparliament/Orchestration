using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationQuestionWrittenAnswerCorrection
{
    public class Transformation : BaseTransformation<Settings>
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

        public override IResource[] TransformSource(string response)
        {
            XDocument doc = XDocument.Parse(response);
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

            ICorrectingAnswer correctingAnswer = new CorrectingAnswer();
            correctingAnswer.Id = GenerateNewId();
            correctingAnswer.AnswerText = new string[] { data.CorrectingAnswerText };
            correctingAnswer.AnswerGivenDate = data.CorrectingDateOfAnswer;
            if (string.IsNullOrWhiteSpace(data.CorrectingAnsweringMemberSesId))
                logger.Warning("No information about correcting minister");
            else
            {
                Uri ministerId = GetMemberId(data.CorrectingAnsweringMemberSesId, data.CorrectingDateOfAnswer, logger);

                if (ministerId != null)
                    correctingAnswer.AnswerHasAnsweringPerson = new IPerson[]
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

            IQuestion question = null;
            if (questionId != null)
            {
                question = new Question()
                {
                    Id = questionId
                };
                correctingAnswer.CorrectingAnswerHasQuestion = new IQuestion[] { question };
            }
            else
            {
                logger.Warning($"Question with Uri ({data.QuestionUri}) not found");
                return null;
            }

            IAnswer originalAnswer = giveMeOriginalAnswer(questionId);
            if (originalAnswer != null)
                correctingAnswer.AnswerReplacesAnswer = originalAnswer;
            else
            {
                logger.Warning($"No answer found to replace for question with Uri ({data.QuestionUri})");
                return null;
            }

            IAnsweringBodyAllocation correctingAnsweringBodyAllocation = giveMeCorrectingAnsweringBodyAllocation(data, questionId);
            if (correctingAnsweringBodyAllocation != null)
            {
                question.QuestionHasAnsweringBodyAllocation = new IAnsweringBodyAllocation[] { correctingAnsweringBodyAllocation };
                correctingAnsweringBodyAllocation.AnsweringBodyAllocationHasAnsweringBody.AnsweringBodyHasWrittenAnswer
                    = new IWrittenAnswer[] { new WrittenAnswer() { Id = correctingAnswer.Id } };
            }
            else
                return null;

            IndexingAndSearchThing iast = new IndexingAndSearchThing();
            var uriElements = doc.Element("response").Element("result").Element("doc").Elements("str").ToList();
            iast.IndexingAndSearchUri = new String[] { uriElements.Where(x => x.Attribute("name").Value == "uri").FirstOrDefault().GetText() };

            return new IResource[] { correctingAnswer, iast };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string correctingAnswerUri = deserializedSource.OfType<IIndexingAndSearchThing>()
                .SingleOrDefault()
                .IndexingAndSearchUri
                .SingleOrDefault();
            return new Dictionary<string, INode>()
            {
                { "correctingAnswerUri", SparqlConstructor.GetNode(correctingAnswerUri) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(IResource[] deserializedSource)
        {
            return new Dictionary<string, INode>()
            {
                { "questionUri", SparqlConstructor.GetNode(questionUriText) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            ICorrectingAnswer correctingAnswer = source.OfType<ICorrectingAnswer>().SingleOrDefault();
            correctingAnswer.Id = subjectUri;
            correctingAnswer.CorrectingAnswerHasQuestion
                .SingleOrDefault()
                .QuestionHasAnsweringBodyAllocation
                .SingleOrDefault()
                .AnsweringBodyAllocationHasAnsweringBody
                .AnsweringBodyHasWrittenAnswer
                .SingleOrDefault()
                .Id = subjectUri;
            IIndexingAndSearchThing iast = source.OfType<IIndexingAndSearchThing>().SingleOrDefault();
            iast.Id = subjectUri;

            return new IResource[] { correctingAnswer, iast };
        }

        private IAnsweringBodyAllocation giveMeCorrectingAnsweringBodyAllocation(Response data, Uri questionId)
        {
            IAnsweringBodyAllocation answeringBodyAllocation = null;

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
                        Id = graph.IsEmpty? GenerateNewId(): (graph.Triples.SingleOrDefault().Object as IUriNode).Uri,
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

        private IAnswer giveMeOriginalAnswer(Uri questionId)
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