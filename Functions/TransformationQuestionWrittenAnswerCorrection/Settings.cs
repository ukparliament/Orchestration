namespace Functions.TransformationQuestionWrittenAnswerCorrection
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
                        ?s a parl:CorrectingAnswer.
                    }
                    where{
                        ?s parl:indexingAndSearchUri @correctingAnswerUri.
                    }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?correctingAnswer a parl:CorrectingAnswer;
                    parl:indexingAndSearchUri ?correctingAnswerUri;
                    parl:answerHasQuestion ?question;
                    parl:answerReplacesAnswer ?answer;
                    parl:answerText ?answerText;
                    parl:answerGivenDate ?answerGivenDate;
                    parl:answerHasAnsweringPerson ?answerHasAnsweringPerson.
                ?question a parl:Question;
                    parl:questionHasAnswer ?answer;
                    parl:questionHasAnswer ?correctingAnswer;
                    parl:questionHasAnsweringBodyAllocation ?questionHasCorrectingAnsweringBodyAllocation.
                ?questionHasCorrectingAnsweringBodyAllocation a parl:AnsweringBodyAllocation;
                    parl:answeringBodyAllocationHasAnsweringBody ?correctingAnsweringBodyAllocationHasAnsweringBody.
                ?correctingAnsweringBodyAllocationHasAnsweringBody a parl:AnsweringBody;
                    parl:answeringBodyHasWrittenAnswer ?correctingAnswer.
            }
            where {
	            bind(@subject as ?correctingAnswer)
	            ?correctingAnswer parl:indexingAndSearchUri ?correctingAnswerUri.
                optional {?correctingAnswer parl:answerHasQuestion ?question.}
                optional {?correctingAnswer parl:answerReplacesAnswer ?answer.}
                optional {?question parl:indexingAndSearchUri ?questionUri.}
                optional {?question parl:questionHasAnswer ?answer.}
                optional {?correctingAnswer parl:answerText ?answerText}
                optional {?correctingAnswer parl:answerGivenDate ?answerGivenDate}
                optional {?correctingAnswer parl:answerHasAnsweringPerson ?answerHasAnsweringPerson}
                optional {?question parl:questionHasAnswer ?correctingAnswer
                optional {
                    ?question parl:questionHasAnsweringBodyAllocation ?questionHasCorrectingAnsweringBodyAllocation.
                    ?questionHasCorrectingAnsweringBodyAllocation parl:answeringBodyAllocationHasAnsweringBody ?correctingAnsweringBodyAllocationHasAnsweringBody.
                    optional {
                        ?correctingAnsweringBodyAllocationHasAnsweringBody parl:answeringBodyHasWrittenAnswer ?correctingAnswer.
                        }
                    }
                }
            }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUri)
        {
            return $"http://13.93.40.140:8983/solr/select?indent=on&version=2.2&q=uri%3A%22{dataUri}%22&fq=&start=0&rows=10&fl=correctedItem_uri%2CansweringDept_ses%2CcorrectingMember_ses%2Ccontent_t%2Cdate_dt%2CleadMember_ses%2Curi&qt=&wt=&explainOther=&hl.fl=";
        }
    }
}
