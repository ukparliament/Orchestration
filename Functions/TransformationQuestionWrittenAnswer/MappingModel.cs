using System;

namespace Functions.TransformationQuestionWrittenAnswer
{
    public class Rootobject
    {
        public Response[] Response { get; set; }
    }

    public class Response
    {
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public int? AnsweringBodyId { get; set; }
        public string AnsweringBody { get; set; }
        public int? UIN { get; set; }
        public DateTime? DueForAnswer { get; set; }
        public DateTime? AnsweredWhen { get; set; }
        public DateTime? TabledWhen { get; set; }
        public string Answer { get; set; }
        public int? AskingMemberId { get; set; }
        public int? AnsweringMinisterId { get; set; }
    }

}
