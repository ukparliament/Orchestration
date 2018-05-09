using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationQuestionWrittenAnswer
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

        public override IResource[] TransformSource(string response)
        {
            XDocument doc = XDocument.Parse(response);
            var questionElements = doc.Element("response").Element("result").Element("doc").Elements("arr").ToList();

            Response data = new Response();
            data.DateTabled = FindXElementByAttributeName(questionElements, "dateTabled_dt", "date").GetDate();
            data.QuestionText = FindXElementByAttributeName(questionElements, "questionText_t", "str").GetText();
            data.AskingMemberSesId = FindXElementByAttributeName(questionElements, "askingMember_ses", "int").GetText();
            data.AnsweringDeptSesId = FindXElementByAttributeName(questionElements, "answeringDept_ses", "int").GetText();
            data.HeadingDueDate = FindXElementByAttributeName(questionElements, "headingDueDate_dt", "date").GetDate();
            data.AnswerText = FindXElementByAttributeName(questionElements, "answerText_t", "str").GetText();
            data.DateOfAnswer = FindXElementByAttributeName(questionElements, "dateOfAnswer_dt", "date").GetDate();
            data.AnsweringMemberSesId = FindXElementByAttributeName(questionElements, "answeringMember_ses", "int").GetText();
            data.DateForAnswer = FindXElementByAttributeName(questionElements, "dateForAnswer_dt", "date").GetDate();

            Question question = new Question();
            question.QuestionAskedAt = data.DateTabled;
            question.QuestionText = data.QuestionText;
            IndexingAndSearchThing iast = new IndexingAndSearchThing();
            var uriElements = doc.Element("response").Element("result").Element("doc").Elements("str").ToList();
            iast.IndexingAndSearchUri = new String[] { uriElements.Where(x => x.Attribute("name").Value == "uri").FirstOrDefault().GetText() };

            if (data.AskingMemberSesId != null)
            {
                Uri memberId = IdRetrieval.GetSubject("sesId", data.AskingMemberSesId, false, logger);
                // Members could share Ses Id. Need to fix.
                if (memberId != null)
                    question.QuestionHasAskingPerson = new IPerson[]
                    {
                            new Person()
                            {
                                Id = memberId
                            }
                    };
                else
                    logger.Warning($"Member with Ses Id ({data.AskingMemberSesId}) not found");
            }

            IAnsweringBodyAllocation answeringBodyAllocation = giveMeAnsweringBodyAllocation(data);
            if (answeringBodyAllocation != null)
                question.QuestionHasAnsweringBodyAllocation = new IAnsweringBodyAllocation[] { answeringBodyAllocation };
            IWrittenAnswerExpectation writtenAnswerExpectation = giveMeWrittenAnswerExpectation(data);
            if (writtenAnswerExpectation != null)
                question.QuestionHasWrittenAnswerExpectation = new IWrittenAnswerExpectation[] { writtenAnswerExpectation };

            return new IResource[] { question, iast };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string writtenQuestionEqmUri = deserializedSource.OfType<IIndexingAndSearchThing>()
                .SingleOrDefault()
                .IndexingAndSearchUri.SingleOrDefault();
            return new Dictionary<string, INode>()
            {
                { "writtenQuestionUri", SparqlConstructor.GetNode(writtenQuestionEqmUri) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IQuestion question = source.OfType<IQuestion>().SingleOrDefault();
            question.Id = subjectUri;
            IIndexingAndSearchThing iast = source.OfType<IIndexingAndSearchThing>().SingleOrDefault();
            iast.Id = subjectUri;
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
                            Id = subjectUri
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
            if ((question.QuestionHasWrittenAnswerExpectation != null) && (question.QuestionHasWrittenAnswerExpectation.Any()))
            {
                IWrittenAnswerExpectation expectationTarget = target.OfType<IWrittenAnswerExpectation>().SingleOrDefault();
                if (expectationTarget != null)
                    question.QuestionHasWrittenAnswerExpectation
                        .SingleOrDefault()
                        .Id = expectationTarget.Id;
            }

            return new IResource[] { question, iast };
        }

        private IAnsweringBodyAllocation giveMeAnsweringBodyAllocation(Response data)
        {
            IAnsweringBodyAllocation answeringBodyAllocation = null;

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
                    IWrittenAnswer writtenAnswer = giveMeWrittenAnswer(data);
                    if (writtenAnswer != null)
                        answeringBodyAllocation
                            .AnsweringBodyAllocationHasAnsweringBody
                            .AnsweringBodyHasWrittenAnswer = new IWrittenAnswer[] { writtenAnswer };
                }
                else
                    logger.Warning($"Answering body with Ses Id ({data.AnsweringDeptSesId}) not found");
            }

            return answeringBodyAllocation;
        }

        private IWrittenAnswer giveMeWrittenAnswer(Response data)
        {
            IWrittenAnswer writtenAnswer = null;

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
                    Uri ministerId = IdRetrieval.GetSubject("sesId", data.AnsweringMemberSesId, false, logger);
                    //members share Ses Id. Need to fix.
                    if (ministerId != null)
                        writtenAnswer.AnswerHasAnsweringPerson = new IPerson[]
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

        private IWrittenAnswerExpectation giveMeWrittenAnswerExpectation(Response data)
        {
            IWrittenAnswerExpectation writtenAnswerExpectation = null;

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