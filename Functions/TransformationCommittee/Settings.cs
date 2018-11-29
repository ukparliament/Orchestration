using System;

namespace Functions.TransformationCommittee
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return null;
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?formalBody a parl:FormalBody;
                parl:formalBodyHasFormalBodyType ?formalBodyHasFormalBodyType;
                parl:formalBodyRemit ?formalBodyRemit;
                parl:formalBodyHasLeadHouse ?formalBodyHasLeadHouse;
                parl:formalBodyHasContactPoint ?formalBodyHasContactPoint.
            ?formalBodyHasContactPoint a parl:ContactPoint;
                parl:email ?email;
                parl:phoneNumber ?phoneNumber.
        }
        where {
            bind(@subject as ?formalBody)
            optional {?formalBody parl:formalBodyHasFormalBodyType ?formalBodyHasFormalBodyType}
            optional {?formalBody parl:formalBodyRemit ?formalBodyRemit}
            optional {?formalBody parl:formalBodyHasLeadHouse ?formalBodyHasLeadHouse}
            optional {
                ?formalBody parl:formalBodyHasContactPoint ?formalBodyHasContactPoint.
                optional {?formalBodyHasContactPoint parl:email ?email}
                optional {?formalBodyHasContactPoint parl:phoneNumber ?phoneNumber}
            }            
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "934ea442-0a9b-491f-99b9-cfb18a016b45").Replace("{id}", dataUrl);
        }
    }
}
