using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationContactPointPersonMnis
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationContactPointPersonMnis";
            }
        }

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
                ?s a parl:ContactPoint.
            }
            where{
                ?s parl:contactPointMnisId @contactPointMnisId.
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
                parl:contactPointMnisId ?contactPointMnisId;
                parl:email ?email;
                parl:faxNumber ?faxNumber;
                parl:phoneNumber ?phoneNumber;
                parl:contactPointHasPostalAddress ?postalAddress;
                parl:contactPointHasPerson ?person.
            ?person a parl:Person.
            ?postalAddress a parl:PostalAddress;
                parl:addressLine1 ?addressLine1;
                parl:addressLine2 ?addressLine2;
                parl:addressLine3 ?addressLine3;
                parl:addressLine4 ?addressLine4;
                parl:addressLine5 ?addressLine5;
                parl:postCode ?postCode.
        }
        where {
            bind(@subject as ?contactPoint)
            ?contactPoint parl:contactPointMnisId ?contactPointMnisId.
            optional {?contactPoint parl:email ?email}
            optional {?contactPoint parl:faxNumber ?faxNumber}
            optional {?contactPoint parl:phoneNumber ?phoneNumber}
            optional {
                ?contactPoint parl:contactPointHasPostalAddress ?postalAddress.
                optional {?postalAddress parl:addressLine1 ?addressLine1}
                optional {?postalAddress parl:addressLine2 ?addressLine2}
                optional {?postalAddress parl:addressLine3 ?addressLine3}
                optional {?postalAddress parl:addressLine4 ?addressLine4}
                optional {?postalAddress parl:addressLine5 ?addressLine5}
                optional {?postalAddress parl:postCode ?postCode}
            }
            optional {?contactPoint parl:contactPointHasPerson ?person}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberAddress_Id,Member_Id,Address1,Address2,Address3,Address4,Address5,Postcode,Email,Phone,Fax";
        }
    }
}
