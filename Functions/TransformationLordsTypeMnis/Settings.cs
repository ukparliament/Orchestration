namespace Functions.TransformationLordsTypeMnis
{
    public class Settings :ITransformationSettings
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
                ?s a parl:HouseIncumbencyType.
            }
            where{
                ?s parl:houseIncumbencyTypeMnisId @houseIncumbencyTypeMnisId.
            }";
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
        	?houseIncumbencyType parl:houseIncumbencyTypeMnisId ?houseIncumbencyTypeMnisId.
            optional {?houseIncumbencyType parl:houseIncumbencyTypeName ?houseIncumbencyTypeName}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=LordsMembershipType_Id,Name";
        }
    }
}
