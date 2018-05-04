using Newtonsoft.Json;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationQuestionWrittenAnswer
{
    public class Transformation : BaseTransformation<Settings>
    {

        public override IResource[] TransformSource(string response)
        {
            Rootobject sourceQuestion = JsonConvert.DeserializeObject<Rootobject>(response, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc });
            IEqmWrittenQuestion question = new EqmWrittenQuestion();

            if ((sourceQuestion != null) && (sourceQuestion.Response != null) &&
                (sourceQuestion.Response.Any()) && (sourceQuestion.Response[0].UIN.HasValue) &&
                (sourceQuestion.Response[0].TabledWhen.HasValue) && (sourceQuestion.Response[0].AskingMemberId.HasValue) &&
                (sourceQuestion.Response[0].AskingMemberId.Value != 0))
            {
                if (sourceQuestion.Response.Length > 1)
                    logger.Warning($"Multiple responses for UIN {sourceQuestion.Response[0].UIN.Value}");
                Response data = sourceQuestion.Response[0];
                question.WrittenQuestionEqmUin = data.UIN.Value.ToString();
                question.QuestionAskedAt = data.TabledWhen;
                question.QuestionText = data.QuestionText;
                Uri memberId = IdRetrieval.GetSubject("memberMnisId", data.AskingMemberId.Value.ToString(), false, logger);
                if (memberId != null)
                    question.QuestionHasAskingPerson = new IPerson[]
                    {
                        new Person()
                        {
                            Id = memberId
                        }
                    };
                else
                    logger.Warning($"Member ({data.AskingMemberId}) not found");
                IAnsweringBodyAllocation answeringBodyAllocation = giveMeAnsweringBodyAllocation(data);
                if (answeringBodyAllocation != null)
                    question.QuestionHasAnsweringBodyAllocation = new IAnsweringBodyAllocation[] { answeringBodyAllocation };
                IWrittenAnswerExpectation writtenAnswerExpectation = giveMeWrittenAnswerExpectation(data);
                if (writtenAnswerExpectation != null)
                    question.QuestionHasWrittenAnswerExpectation = new IWrittenAnswerExpectation[] { writtenAnswerExpectation };
            }

            return new IResource[] { question };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string writtenQuestionEqmUin = deserializedSource.OfType<IEqmWrittenQuestion>()
                .SingleOrDefault()
                .WrittenQuestionEqmUin;
            DateTimeOffset questionAskedAt = deserializedSource.OfType<IQuestion>()
                .SingleOrDefault()
                .QuestionAskedAt
                .Value;
            return new Dictionary<string, INode>()
            {
                { "writtenQuestionEqmUin", SparqlConstructor.GetNode(writtenQuestionEqmUin) },
                {"", SparqlConstructor.GetNodeDate(questionAskedAt) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IEqmWrittenQuestion question = source.OfType<IEqmWrittenQuestion>().SingleOrDefault();
            question.Id = subjectUri;
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

            return new IResource[] { question };
        }

        private IAnsweringBodyAllocation giveMeAnsweringBodyAllocation(Response data)
        {
            IAnsweringBodyAllocation answeringBodyAllocation = null;

            if (data.AnsweringBodyId.HasValue)
            {
                Uri answeringBodyId = IdRetrieval.GetSubject("answeringBodyMnisId", data.AnsweringBodyId.Value.ToString(), false, logger);
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
                    logger.Warning($"Answering body ({data.AskingMemberId}) not found");
            }

            return answeringBodyAllocation;
        }

        private IWrittenAnswer giveMeWrittenAnswer(Response data)
        {
            IWrittenAnswer writtenAnswer = null;

            if ((string.IsNullOrWhiteSpace(data.Answer) == false) || (data.AnsweredWhen.HasValue))
            {
                writtenAnswer = new WrittenAnswer()
                {
                    Id = GenerateNewId(),
                    AnswerText = DeserializerHelper.GiveMeSingleTextValue(data.Answer),
                    AnswerGivenDate = data.AnsweredWhen
                };
                if (data.AnsweringMinisterId.HasValue)
                {
                    Uri ministerId = IdRetrieval.GetSubject("memberMnisId", data.AnsweringMinisterId.Value.ToString(), false, logger);
                    if (ministerId != null)
                        writtenAnswer.AnswerHasAnsweringPerson = new IPerson[]
                            {
                                new Person()
                                {
                                    Id=ministerId
                                }
                            };
                    else
                        logger.Warning($"Minister ({data.AskingMemberId}) not found");
                }
            }

            return writtenAnswer;
        }

        private IWrittenAnswerExpectation giveMeWrittenAnswerExpectation(Response data)
        {
            IWrittenAnswerExpectation writtenAnswerExpectation = null;

            if (data.DueForAnswer.HasValue)
                writtenAnswerExpectation = new WrittenAnswerExpectation()
                {
                    Id = GenerateNewId(),
                    AnswerExpectationStartDate = DeserializerHelper.GiveMeSingleDateValue(data.DueForAnswer)
                };

            return writtenAnswerExpectation;
        }

    }
}