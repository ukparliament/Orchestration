using System;

namespace Functions.TransformationQuestionWrittenAnswer
{
    public class Rootobject
    {
        public Response[] Response { get; set; }
    }

    public class Response
    {
        public DateTimeOffset? DateTabled { get; set; }
        public string QuestionHeading { get; set; }
        public string QuestionText { get; set; }
        public string AskingMemberSesId { get; set; }
        public string AnsweringDeptSesId { get; set; }
        public DateTimeOffset? HeadingDueDate { get; set; }
        public string AnswerText { get; set; }
        public DateTimeOffset? DateOfAnswer { get; set; }
        public string AnsweringMemberSesId { get; set; }
        public DateTimeOffset? DateForAnswer { get; set; }
    }

}
