﻿using System;

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
                return @"
            construct{
                ?s a parl:FormalBody.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:formalBodyName ?formalBodyName.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?formalBody a parl:FormalBody;
                parl:formalBodyRemit ?formalBodyRemit;
                parl:formalBodyHasLeadHouse ?formalBodyHasLeadHouse;
                parl:formalBodyHasContactPoint ?formalBodyHasContactPoint.
            ?formalBodyHasContactPoint a parl:ContactPoint;
                parl:email ?email;
                parl:phoneNumber ?phoneNumber.
        }
        where {
            bind(@subject as ?formalBody)
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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "035fa54f-492a-4008-93da-5ee912a2d0cf").Replace("{id}", dataUrl);
        }
    }
}