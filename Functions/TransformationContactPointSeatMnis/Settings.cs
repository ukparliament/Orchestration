using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationContactPointSeatMnis
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationContactPointSeatMnis";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                sourceXmlNamespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                sourceXmlNamespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                sourceXmlNamespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                return sourceXmlNamespaceManager;
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
                ?s a parl:ContactPoint; 
                    parl:contactPointMnisId @contactPointMnisId.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "contactPointMnisId","atom:entry/atom:content/m:properties/d:MemberAddress_Id"}
                };
            }
        }

        public string ExisitngGraphSparqlCommand
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
                parl:contactPointHasPostalAddress ?postalAddress.
            ?postalAddress a parl:PostalAddress;
                parl:addressLine1 ?addressLine1;
                parl:addressLine2 ?addressLine2;
                parl:addressLine3 ?addressLine3;
                parl:addressLine4 ?addressLine4;
                parl:addressLine5 ?addressLine5;
                parl:postCode ?postCode.
            ?incumbency parl:incumbencyHasContactPoint ?contactPoint.
        }
        where {
            bind(@subject as ?contactPoint)
            ?contactPoint a parl:ContactPoint;
                parl:contactPointMnisId ?contactPointMnisId.
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
            optional {?incumbency parl:incumbencyHasContactPoint ?contactPoint}
        }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return null;
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberAddress_Id,Member_Id,Address1,Address2,Address3,Address4,Address5,Postcode,Email,Phone,Fax";
        }
    }
}
