namespace Functions.TransformationQuestionWrittenAnswer
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/json";
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Question.
            }
            where{
                ?s parl:writtenQuestionEqmUin @writtenQuestionEqmUin;
                    parl:questionAskedAt @questionAskedAt.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?question a parl:EqmWrittenQuestion;
                    parl:writtenQuestionEqmUin ?writtenQuestionEqmUin;
	                parl:questionAskedAt ?questionAskedAt;
		            parl:questionText ?questionText;
                    parl:questionHasAskingPerson ?questionHasAskingPerson;
                    parl:questionHasAnsweringBodyAllocation ?questionHasAnsweringBodyAllocation;
                    parl:questionHasWrittenAnswerExpectation ?questionHasWrittenAnswerExpectation.
                ?questionHasAskingPerson a parl:Member.
                ?questionHasAnsweringBodyAllocation a parl:AnsweringBodyAllocation;
                    parl:answeringBodyAllocationHasAnsweringBody ?answeringBodyAllocationHasAnsweringBody.
                ?answeringBodyAllocationHasAnsweringBody a parl:AnsweringBody;
                    parl:answeringBodyHasWrittenAnswer ?answeringBodyHasWrittenAnswer.
                ?answeringBodyHasWrittenAnswer a parl:WrittenAnswer;
                    parl:answerHasQuestion ?question;
                    parl:answerText ?answerText;
                    parl:answerGivenDate ?answerGivenDate;
                    parl:answerHasAnsweringPerson ?answerHasAnsweringPerson.
                ?questionHasWrittenAnswerExpectation a parl:WrittenAnswerExpectation;
                    parl:answerExpectationStartDate ?answerExpectationStartDate.
            }
            where {
	            bind(@subject as ?question)
	            ?question parl:writtenQuestionEqmUin ?writtenQuestionEqmUin;
                    parl:questionAskedAt ?questionAskedAt.
	            optional {?question parl:questionText ?questionText}
                optional {?question parl:questionHasAskingPerson ?questionHasAskingPerson}
                optional {
                    ?question parl:questionHasAnsweringBodyAllocation ?questionHasAnsweringBodyAllocation.
                    ?questionHasAnsweringBodyAllocation parl:answeringBodyAllocationHasAnsweringBody ?answeringBodyAllocationHasAnsweringBody.
                    optional {
                        ?answeringBodyAllocationHasAnsweringBody parl:answeringBodyHasWrittenAnswer ?answeringBodyHasWrittenAnswer.
                        ?answeringBodyHasWrittenAnswer parl:answerHasQuestion ?question.
                        optional {?answeringBodyHasWrittenAnswer parl:answerText ?answerText}
                        optional {?answeringBodyHasWrittenAnswer parl:answerGivenDate ?answerGivenDate}
                        optional {?answeringBodyHasWrittenAnswer parl:answerHasAnsweringPerson ?answerHasAnsweringPerson}
                    }
                }
                optional {
                    ?question parl:questionHasWrittenAnswerExpectation ?questionHasWrittenAnswerExpectation.
                    ?questionHasWrittenAnswerExpectation parl:answerExpectationStartDate ?answerExpectationStartDate.
                }
            }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://eqm-services.digiminster.com/writtenquestions/list?parameters.uINs={dataUrl}";
        }
    }
}
