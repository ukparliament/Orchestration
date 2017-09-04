using System;

namespace Functions.TransformationEPetition
{

    public class Rootobject
    {
        public Links links { get; set; }
        public Data data { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
    }

    public class Data
    {
        public string type { get; set; }
        public int id { get; set; }
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public string action { get; set; }
        public string background { get; set; }
        public string additional_details { get; set; }
        public string state { get; set; }
        public int signature_count { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? open_at { get; set; }
        public DateTime? closed_at { get; set; }
        public DateTime? government_response_at { get; set; }
        public DateTime? scheduled_debate_date { get; set; }
        public DateTime? debate_threshold_reached_at { get; set; }
        public DateTime? rejected_at { get; set; }
        public DateTime? debate_outcome_at { get; set; }
        public DateTime? moderation_threshold_reached_at { get; set; }
        public DateTime? response_threshold_reached_at { get; set; }
        public string creator_name { get; set; }
        public Government_Response government_response { get; set; }
        public Debate debate { get; set; }
        public Rejection rejection { get; set; }
        public Signatures_By_Country[] signatures_by_country { get; set; }
        public Signatures_By_Constituency[] signatures_by_constituency { get; set; }        
    }

    public class Government_Response
    {
        public string summary { get; set; }
        public string details { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Debate
    {
        public DateTime? debated_on { get; set; }
        public string transcript_url { get; set; }
        public string video_url { get; set; }
        public string overview { get; set; }
    }

    public class Rejection
    {
        public string code { get; set; }
        public string details { get; set; }
    }

    public class Signatures_By_Country
    {
        public string name { get; set; }
        public string code { get; set; }
        public int signature_count { get; set; }
    }

    public class Signatures_By_Constituency
    {
        public string name { get; set; }
        public string ons_code { get; set; }
        public string mp { get; set; }
        public int signature_count { get; set; }
    }

}
