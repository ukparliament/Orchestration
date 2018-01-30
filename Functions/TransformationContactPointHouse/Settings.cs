﻿using System;

namespace Functions.TransformationContactPointHouse
{
    public class Settings : ITransformationSettings
    {
        public string AcceptHeader
        {
            get
            {
                return string.Empty;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:ContactPoint.
            }
            where{
                ?s parl:contactPointHasHouse @contactPointHasHouse.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?contactPoint a parl:ContactPoint;
        	    parl:contactPointHasHouse ?contactPointHasHouse;
                parl:contactPointHasPostalAddress ?contactPointHasPostalAddress.
            ?contactPointHasHouse a parl:House.
            ?contactPointHasPostalAddress a parl:PostalAddress;
                parl:addressLine1 ?addressLine1;
                parl:addressLine2 ?addressLine2;
                parl:addressLine3 ?addressLine3;
                parl:addressLine4 ?addressLine4;
                parl:addressLine5 ?addressLine5;
                parl:postCode ?postCode.
        }
        where {
            bind(@subject as ?contactPoint)
            ?contactPoint parl:contactPointHasHouse ?contactPointHasHouse.
            optional {
                ?contactPoint parl:contactPointHasPostalAddress ?contactPointHasPostalAddress.
                optional {?contactPointHasPostalAddress parl:addressLine1 ?addressLine1}
                optional {?contactPointHasPostalAddress parl:addressLine2 ?addressLine2}
                optional {?contactPointHasPostalAddress parl:addressLine3 ?addressLine3}
                optional {?contactPointHasPostalAddress parl:addressLine4 ?addressLine4}
                optional {?contactPointHasPostalAddress parl:addressLine5 ?addressLine5}
                optional {?contactPointHasPostalAddress parl:postCode ?postCode}
            }
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "261f09cd-9c4e-4e76-a928-b7b8c5b3cdcd").Replace("{id}", dataUrl);
        }
    }
}
