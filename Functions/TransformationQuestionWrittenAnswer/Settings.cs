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
                ?s parl:indexingAndSearchUri @writtenQuestionUri.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?question 
                    parl:indexingAndSearchUri ?writtenQuestionUri.
                ?question a parl:Question;
	                parl:questionAskedAt ?questionAskedAt;
                    parl:questionHeading ?questionHeading;
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
	            ?question parl:indexingAndSearchUri ?writtenQuestionUri.
                optional {?question parl:questionAskedAt ?questionAskedAt}
                optional {?question parl:questionHeading ?questionHeading}
	            optional {?question parl:questionText ?questionText}
                optional {?question parl:questionHasAskingPerson ?questionHasAskingPerson}
                optional {
                    ?question parl:questionHasAnsweringBodyAllocation ?questionHasAnsweringBodyAllocation.
                    ?questionHasAnsweringBodyAllocation parl:answeringBodyAllocationHasAnsweringBody ?answeringBodyAllocationHasAnsweringBody.
                    optional {
                        ?answeringBodyAllocationHasAnsweringBody parl:answeringBodyHasWrittenAnswer ?answeringBodyHasWrittenAnswer.
                        ?answeringBodyHasWrittenAnswer parl:answerHasQuestion ?question.
                        optional {?answeringBodyHasWrittenAnswer parl:answerReplacesAnswer ?answer}
                        filter (bound(?answer) = false)
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

        public string FullDataUrlParameterizedString(string dataUri)
        {
            return $"http://13.93.40.140:8983/solr/select?indent=on&version=2.2&q=uri%3A%22{dataUri}%22&fq=&start=0&rows=10&fl=dateTabled_dt%2CquestionText_t%2Ctitle_t%2CaskingMember_ses%2CansweringDept_ses%2CheadingDueDate_dt%2CanswerText_t%2CdateOfAnswer_dt%2CansweringMember_ses%2CdateForAnswer_dt%2Curi&qt=&wt=&explainOther=&hl.fl=";
        }
    }
}
