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
            var questionElements = doc.Element("response").Element("result").Element("doc").Elements("arr").ToList();

            Response data = new Response();
            data.QuestionUri = FindXElementByAttributeName(questionElements, "correctedItem_uri", "str").GetText();
            data.CorrectingAnsweringDeptSesId = FindXElementByAttributeName(questionElements, "answeringDept_ses", "int").GetText();
            data.CorrectingAnsweringMemberSesId = FindXElementByAttributeName(questionElements, "correctingMember_ses", "int").GetText();
            data.CorrectingAnswerText = FindXElementByAttributeName(questionElements, "content_t", "str").GetText();

            var dateElements = doc.Element("response").Element("result").Element("doc").Elements("date").ToList();
            data.CorrectingDateOfAnswer = dateElements.Where(x => x.Attribute("name").Value == "date_dt").FirstOrDefault().GetDate();

            WrittenAnswer correctingAnswer = new WrittenAnswer();
            correctingAnswer.Id = GenerateNewId();
            correctingAnswer.AnswerText = new string[] { data.CorrectingAnswerText };
            correctingAnswer.AnswerGivenDate = data.CorrectingDateOfAnswer;
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

            Uri questionId = IdRetrieval.GetSubject("indexingAndSearchUri", data.QuestionUri, false, logger);
            
            IQuestion question = giveMeCorrectingAnsweringQuestion(data, correctingAnswer);
            //correctingAnswer.AnswerHasQuestion = new Question[] { question };
            if (question != null)
            {
                //question.QuestionHasCorrectingAnswer = new ICorrectingAnswer[] { correctingAnswer };
                correctingAnswer.AnswerHasQuestion = new IQuestion[] { question };
                correctingAnswer.AnswerReplacesAnswer = question.QuestionHasAnswer.ToList().First();
            }
            else
                return null;
            
            IAnsweringBodyAllocation correctingAnsweringBodyAllocation = giveMeCorrectingAnsweringBodyAllocation(data);
            if (correctingAnsweringBodyAllocation != null)
            {
                question.QuestionHasAnsweringBodyAllocation = new IAnsweringBodyAllocation[] { correctingAnsweringBodyAllocation };
                correctingAnsweringBodyAllocation.AnsweringBodyAllocationHasAnsweringBody.AnsweringBodyHasWrittenAnswer
                    = new IWrittenAnswer[] { (IWrittenAnswer)correctingAnswer };
            }
            else
                return null;

            IndexingAndSearchThing iast = new IndexingAndSearchThing();
            var uriElements = doc.Element("response").Element("result").Element("doc").Elements("str").ToList();
            iast.IndexingAndSearchUri = new String[] { uriElements.Where(x => x.Attribute("name").Value == "uri").FirstOrDefault().GetText() };

            return new IResource[] { correctingAnswer,  iast };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string correctingAnswerUri = deserializedSource.OfType<IIndexingAndSearchThing>()
                .SingleOrDefault()
                .IndexingAndSearchUri.SingleOrDefault();
            return new Dictionary<string, INode>()
            {
                { "correctingAnswerUri", SparqlConstructor.GetNode(correctingAnswerUri) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IAnswer correctingAnswer = source.OfType<IAnswer>().SingleOrDefault();
            correctingAnswer.Id = subjectUri;
            IIndexingAndSearchThing iast = source.OfType<IIndexingAndSearchThing>().SingleOrDefault();
            iast.Id = subjectUri;
            IQuestion question = correctingAnswer.AnswerHasQuestion.SingleOrDefault();
            //question.Id = correctingAnswer.AnswerHasQuestion.FirstOrDefault().Id;
            if ((question.QuestionHasAnsweringBodyAllocation != null) && (question.QuestionHasAnsweringBodyAllocation.Any()))
            {
                IAnsweringBodyAllocation answeringBodyAllocationTarget = target.OfType<IAnsweringBodyAllocation>().SingleOrDefault();
                if (answeringBodyAllocationTarget != null)
                    question.QuestionHasAnsweringBodyAllocation.SingleOrDefault().Id = answeringBodyAllocationTarget.Id;
                if (question.QuestionHasAnsweringBodyAllocation.SingleOrDefault()?.AnsweringBodyAllocationHasAnsweringBody?.AnsweringBodyHasWrittenAnswer != null)
                {
                    question.QuestionHasAnsweringBodyAllocation
                    .SingleOrDefault()
                    .AnsweringBodyAllocationHasAnsweringBody
                    .AnsweringBodyHasWrittenAnswer
                    .SingleOrDefault()
                    .AnswerHasQuestion = new IQuestion[]
                    {
                        new Question()
                        {
                            Id = question.Id
                        }
                    };
                    IWrittenAnswer answerTarget = target.OfType<IWrittenAnswer>().SingleOrDefault();
                    if (answerTarget != null)
                    {
                        question.QuestionHasAnsweringBodyAllocation
                            .SingleOrDefault()
                            .AnsweringBodyAllocationHasAnsweringBody
                            .AnsweringBodyHasWrittenAnswer
                            .SingleOrDefault()
                            .Id = answerTarget.Id;
                    }
                }
            }
            return new IResource[] { correctingAnswer, question, iast };
        }

        private IQuestion giveMeCorrectingAnsweringQuestion(Response data, WrittenAnswer correctingAnswer)
        {
            IQuestion question = null;

            if (data.QuestionUri != null)
            {
                Uri questionId = IdRetrieval.GetSubject("indexingAndSearchUri", data.QuestionUri, false, logger);
                if (questionId != null)
                {
                    question = new Question()
                    {
                        Id = questionId
                    };
                    string command = @"
                        construct{
                            ?answer parl:answerHasQuestion @question.
                        }
                        where{
                            ?answer parl:answerHasQuestion @question.
                        }";
                    SparqlParameterizedString sparql = new SparqlParameterizedString(command);
                    sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                    sparql.SetUri("question", questionId);
                    IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "true");
                    IEnumerable<INode> nodes = graph.Triples.SubjectNodes;

                    Uri questionAnswerId = ((IUriNode)(nodes.FirstOrDefault())).Uri;
                    question.QuestionHasAnswer = new IAnswer[] { (IAnswer)(new Answer() { Id = questionAnswerId }), correctingAnswer };
                }
                else
                    logger.Warning($"Question with Uri ({data.QuestionUri}) not found");
            }

            return question;
        }

        private IAnsweringBodyAllocation giveMeCorrectingAnsweringBodyAllocation(Response data)
        {
            IAnsweringBodyAllocation answeringBodyAllocation = null;

            if (data.CorrectingAnsweringDeptSesId != null)
            {
                Uri answeringBodyId = IdRetrieval.GetSubject("sesId", data.CorrectingAnsweringDeptSesId, false, logger);
                if (answeringBodyId != null)
                {
                    answeringBodyAllocation = new AnsweringBodyAllocation()
                    {
                        Id = GenerateNewId(),
                        AnsweringBodyAllocationHasAnsweringBody = new AnsweringBody()
                        {
                            Id = answeringBodyId
                        }
                    };
                    //IWrittenAnswer writtenAnswer = giveMeCorrectingAnswer(data);
                    //if (writtenAnswer != null)
                    //    answeringBodyAllocation
                    //        .AnsweringBodyAllocationHasAnsweringBody
                    //        .AnsweringBodyHasWrittenAnswer = new IWrittenAnswer[] { writtenAnswer };
                }
                else
                    logger.Warning($"Answering body with Ses Id ({data.CorrectingAnsweringDeptSesId}) not found");
            }

            return answeringBodyAllocation;
        }

        //private IWrittenAnswer giveMeCorrectingAnswer(Response data)
        //{
        //    IWrittenAnswer writtenAnswer = null;

        //    if ((string.IsNullOrWhiteSpace(data.CorrectingAnswerText) == false) || (data.CorrectingDateOfAnswer.HasValue))
        //    {
        //        writtenAnswer = new WrittenAnswer()
        //        {
        //            Id = GenerateNewId(),
        //            AnswerText = DeserializerHelper.GiveMeSingleTextValue(data.CorrectingAnswerText),
        //            AnswerGivenDate = data.CorrectingDateOfAnswer
        //        };
        //        if (!string.IsNullOrWhiteSpace(data.CorrectingAnsweringMemberSesId))
        //        {
        //            Uri ministerId = GetMemberId(data.CorrectingAnsweringMemberSesId, data.CorrectingDateOfAnswer, logger);

        //            if (ministerId != null)
        //                writtenAnswer.AnswerHasAnsweringPerson = new IPerson[]
        //                    {
        //                        new Person()
        //                        {
        //                            Id=ministerId
        //                        }
        //                    };
        //            else
        //                logger.Warning($"Minister with Ses Id ({data.CorrectingAnsweringMemberSesId}) not found");
        //        }
        //    }

        //    return writtenAnswer;
        //}
    }
}