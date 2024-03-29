﻿using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationQuestionWrittenAnswer
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
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
        public override BaseResource[] TransformSource(XDocument doc)
        {
            var arrElements = doc?.Element("response")?.Element("result")?.Element("doc")?.Elements("arr")?.ToList();
            var strElements = doc?.Element("response")?.Element("result")?.Element("doc")?.Elements("str")?.ToList();
            if ((arrElements == null) || (strElements == null))
                return null;

            Response data = new Response();
            data.DateTabled = FindXElementByAttributeName(arrElements, "dateTabled_dt", "date").GetDate();
            data.QuestionText = FindXElementByAttributeName(arrElements, "questionText_t", "str").GetText();
            data.QuestionHeading = strElements.Where(x => x.Attribute("name").Value == "title_t").FirstOrDefault().GetText();
            data.AskingMemberSesId = FindXElementByAttributeName(arrElements, "askingMember_ses", "int").GetText();
            data.AnsweringDeptSesId = FindXElementByAttributeName(arrElements, "answeringDept_ses", "int").GetText();
            data.HeadingDueDate = FindXElementByAttributeName(arrElements, "headingDueDate_dt", "date").GetDate();
            data.AnswerText = FindXElementByAttributeName(arrElements, "answerText_t", "str").GetText();
            data.DateOfAnswer = FindXElementByAttributeName(arrElements, "dateOfAnswer_dt", "date").GetDate();
            data.AnsweringMemberSesId = FindXElementByAttributeName(arrElements, "answeringMember_ses", "int").GetText();
            data.DateForAnswer = FindXElementByAttributeName(arrElements, "dateForAnswer_dt", "date").GetDate();
            data.Uin = FindXElementByAttributeName(arrElements, "uin_t", "str").GetText();

            IndexingAndSearchWrittenQuestion question = new IndexingAndSearchWrittenQuestion();
            question.QuestionAskedAt = data.DateTabled;
            question.QuestionHeading = data.QuestionHeading;
            question.QuestionText = data.QuestionText;
            if (string.IsNullOrWhiteSpace(data.Uin) == false)
                question.WrittenQuestionIndexingAndSearchUin = new string[] { data.Uin };

            question.IndexingAndSearchUri = new String[] { strElements.Where(x => x.Attribute("name").Value == "uri").FirstOrDefault().GetText() };

            if (data.AskingMemberSesId != null)
            {
                Uri memberId = GetMemberId(data.AskingMemberSesId, data.DateTabled == null ? data.DateOfAnswer : data.DateTabled, logger);
                if (memberId != null)
                    question.QuestionHasAskingPerson = new Person[]
                    {
                            new Person()
                            {
                                Id = memberId
                            }
                    };
                else
                    logger.Warning($"Member with Ses Id ({data.AskingMemberSesId}) not found");
            }

            AnsweringBodyAllocation answeringBodyAllocation = giveMeAnsweringBodyAllocation(data);
            if (answeringBodyAllocation != null)
                question.QuestionHasAnsweringBodyAllocation = new AnsweringBodyAllocation[] { answeringBodyAllocation };
            WrittenAnswerExpectation writtenAnswerExpectation = giveMeWrittenAnswerExpectation(data);
            if (writtenAnswerExpectation != null)
                question.QuestionHasWrittenAnswerExpectation = new WrittenAnswerExpectation[] { writtenAnswerExpectation };

            return new BaseResource[] { question };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string writtenQuestionUri = deserializedSource.Where(x => x.GetType() == typeof(IndexingAndSearchWrittenQuestion))
                .OfType<IndexingAndSearchWrittenQuestion>()
                .SingleOrDefault()
                .IndexingAndSearchUri.SingleOrDefault();
            DateTimeOffset askedDate = deserializedSource.OfType<IndexingAndSearchWrittenQuestion>()
                .SingleOrDefault().
                QuestionAskedAt.GetValueOrDefault();
            string writtenQuestionUin = deserializedSource.OfType<IndexingAndSearchWrittenQuestion>()
                .SingleOrDefault().
                WrittenQuestionIndexingAndSearchUin?.SingleOrDefault();
            return new Dictionary<string, INode>()
            {
                { "writtenQuestionUri", SparqlConstructor.GetNode(writtenQuestionUri) },
                { "writtenQuestionUin", SparqlConstructor.GetNode(writtenQuestionUin !=null ? writtenQuestionUin : writtenQuestionUri)},
                { "askedDate", SparqlConstructor.GetNodeDate(askedDate)},
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            IndexingAndSearchWrittenQuestion question = source.OfType<IndexingAndSearchWrittenQuestion>().SingleOrDefault();
            question.Id = subjectUri;
            if ((question.QuestionHasAnsweringBodyAllocation != null) && (question.QuestionHasAnsweringBodyAllocation.Any()))
            {
                AnsweringBodyAllocation answeringBodyAllocationTarget = target.OfType<AnsweringBodyAllocation>().SingleOrDefault();
                if (answeringBodyAllocationTarget != null)
                    question.QuestionHasAnsweringBodyAllocation.SingleOrDefault().Id = answeringBodyAllocationTarget.Id;
                if (question.QuestionHasAnsweringBodyAllocation.SingleOrDefault()?.AnsweringBodyAllocationHasAnsweringBody?.AnsweringBodyHasWrittenAnswer != null)
                {
                    question.QuestionHasAnsweringBodyAllocation
                    .SingleOrDefault()
                    .AnsweringBodyAllocationHasAnsweringBody
                    .AnsweringBodyHasWrittenAnswer
                    .SingleOrDefault()
                    .AnswerHasQuestion = new Question[]
                    {
                        new Question()
                        {
                            Id = subjectUri
                        }
                    };
                    WrittenAnswer answerTarget = target.OfType<WrittenAnswer>().SingleOrDefault();
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
            if ((question.QuestionHasWrittenAnswerExpectation != null) && (question.QuestionHasWrittenAnswerExpectation.Any()))
            {
                WrittenAnswerExpectation expectationTarget = target.OfType<WrittenAnswerExpectation>().SingleOrDefault();
                if (expectationTarget != null)
                    question.QuestionHasWrittenAnswerExpectation
                        .SingleOrDefault()
                        .Id = expectationTarget.Id;
            }

            return new BaseResource[] { question };
        }

        private AnsweringBodyAllocation giveMeAnsweringBodyAllocation(Response data)
        {
            AnsweringBodyAllocation answeringBodyAllocation = null;

            if (data.AnsweringDeptSesId != null)
            {
                Uri answeringBodyId = IdRetrieval.GetSubject("sesId", data.AnsweringDeptSesId, false, logger);
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
                    WrittenAnswer writtenAnswer = giveMeWrittenAnswer(data);
                    if (writtenAnswer != null)
                        answeringBodyAllocation
                            .AnsweringBodyAllocationHasAnsweringBody
                            .AnsweringBodyHasWrittenAnswer = new WrittenAnswer[] { writtenAnswer };
                }
                else
                    logger.Warning($"Answering body with Ses Id ({data.AnsweringDeptSesId}) not found");
            }

            return answeringBodyAllocation;
        }

        private WrittenAnswer giveMeWrittenAnswer(Response data)
        {
            WrittenAnswer writtenAnswer = null;

            if ((string.IsNullOrWhiteSpace(data.AnswerText) == false) || (data.DateOfAnswer.HasValue))
            {
                writtenAnswer = new WrittenAnswer()
                {
                    Id = GenerateNewId(),
                    AnswerText = DeserializerHelper.GiveMeSingleTextValue(data.AnswerText),
                    AnswerGivenDate = data.DateOfAnswer
                };
                if (!string.IsNullOrWhiteSpace(data.AnsweringMemberSesId))
                {
                    Uri ministerId = GetMemberId(data.AnsweringMemberSesId, data.DateOfAnswer, logger);

                    if (ministerId != null)
                        writtenAnswer.AnswerHasAnsweringPerson = new Person[]
                            {
                                new Person()
                                {
                                    Id=ministerId
                                }
                            };
                    else
                        logger.Warning($"Minister with Ses Id ({data.AnsweringMemberSesId}) not found");
                }
            }

            return writtenAnswer;
        }

        private WrittenAnswerExpectation giveMeWrittenAnswerExpectation(Response data)
        {
            WrittenAnswerExpectation writtenAnswerExpectation = null;

            if (data.DateForAnswer.HasValue)
                writtenAnswerExpectation = new WrittenAnswerExpectation()
                {
                    Id = GenerateNewId(),
                    AnswerExpectationStartDate = DeserializerHelper.GiveMeSingleDateValue(data.DateForAnswer)
                };

            return writtenAnswerExpectation;
        }

    }
}