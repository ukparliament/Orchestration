using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationLordsTypeMnis
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationLordsTypeMnis";
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
                ?s a parl:HouseIncumbencyType.
            }
            where{
                ?s a parl:HouseIncumbencyType; 
                    parl:houseIncumbencyTypeMnisId @houseIncumbencyTypeMnisId.
            }";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"houseIncumbencyTypeMnisId","atom:entry/atom:content/m:properties/d:LordsMembershipType_Id" }
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?houseIncumbencyType a parl:HouseIncumbencyType;
                parl:houseIncumbencyTypeMnisId ?houseIncumbencyTypeMnisId;
                parl:houseIncumbencyTypeName ?houseIncumbencyTypeName.
        }
        where {
            bind(@subject as ?houseIncumbencyType)
        	?houseIncumbencyType a parl:HouseIncumbencyType.
            optional {?houseIncumbencyType parl:houseIncumbencyTypeMnisId ?houseIncumbencyTypeMnisId}
            optional {?houseIncumbencyType parl:houseIncumbencyTypeName ?houseIncumbencyTypeName}
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
            return $"{dataUrl}?$select=LordsMembershipType_Id,Name";
        }
    }
}
