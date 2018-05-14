using System;

namespace Functions.TransformationQuestionWrittenAnswerCorrection
{
    public class Rootobject
    {
        public Response[] Response { get; set; }
    }

    public class Response
    {
        public string QuestionUri { get; set; }
        public string CorrectingAnsweringDeptSesId { get; set; }
        public string CorrectingAnsweringMemberSesId { get; set; }
        public string CorrectingAnswerText { get; set; }
        public DateTimeOffset? CorrectingDateOfAnswer { get; set; }

    }

}
