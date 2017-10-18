namespace Functions.TransformationHouseIncumbencyMnis
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
                ?s a parl:ParliamentaryIncumbency.
            }
            where{
                ?s parl:houseIncumbencyMnisId @houseIncumbencyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?houseIncumbency a parl:ParliamentaryIncumbency;
                parl:houseIncumbencyMnisId ?houseIncumbencyMnisId;
                parl:parliamentaryIncumbencyHasMember ?member;
                parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate;
                parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate;
                parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse;
                parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType.
        }
        where {
            bind(@subject as ?houseIncumbency)
            ?houseIncumbency parl:houseIncumbencyMnisId ?houseIncumbencyMnisId.
            optional {?houseIncumbency parl:parliamentaryIncumbencyHasMember ?member}
            optional {?houseIncumbency parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate}
            optional {?houseIncumbency parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate}
            optional {?houseIncumbency parl:houseIncumbencyHasHouse ?houseIncumbencyHasHouse}
            optional {?houseIncumbency parl:houseIncumbencyHasHouseIncumbencyType ?houseIncumbencyHasHouseIncumbencyType}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberLordsMembershipType_Id,Member_Id,LordsMembershipType_Id,StartDate,EndDate";
        }
    }
}
