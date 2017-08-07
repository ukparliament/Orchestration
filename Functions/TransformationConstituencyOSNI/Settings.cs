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

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:ConstituencyGroup.
            }
            where{
                ?s parl:constituencyGroupOnsCode @constituencyGroupOnsCode.
            }";
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
            ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.
            optional {
                ?constituencyGroup parl:constituencyGroupHasConstituencyArea ?constituencyArea
                optional {?constituencyArea parl:constituencyAreaExtent ?constituencyAreaExtent}
            }
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://gisservices.spatialni.gov.uk/arcgisc/rest/services/OpenData/OSNIOpenData_50KBoundaries/MapServer/3/query?f=json&outSR=4326&outFields=PC_ID,PC_NAME&where=OBJECTID%3D{dataUrl}";
        }
    }
}