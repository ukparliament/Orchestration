using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationConstituencyOSNI
{
    public class Settings : ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationConstituencyOSNI";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/json";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:ConstituencyGroup.
            }
            where{
                ?s a parl:ConstituencyGroup; 
                    parl:constituencyGroupOnsCode @constituencyGroupOnsCode.
            }";
            }
        }

        private Dictionary<string, string> subjectRetrievalParameters;
        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"constituencyGroupOnsCode","root/features/attributes/PC_ID" }
                };
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent.
        }
        where {
            bind(@subject as ?constituencyGroup)
            ?constituencyGroup a parl:ConstituencyGroup;
                parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                parl:constituencyGroupHasConstituencyArea ?constituencyArea.
            ?constituencyArea a parl:ConstituencyArea;
                parl:constituencyAreaExtent ?constituencyAreaExtent.
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
            return $"https://gisservices.spatialni.gov.uk/arcgisc/rest/services/OpenData/OSNIOpenData_50KBoundaries/MapServer/3/query?f=json&outSR=4326&outFields=PC_ID,PC_NAME&where=OBJECTID%3D{dataUrl}";
        }
    }
}