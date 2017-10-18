namespace Functions.TransformationSeatIncumbencyMnis
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
                ?s parl:seatIncumbencyMnisId @seatIncumbencyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?seatIncumbency a parl:ParliamentaryIncumbency;
                parl:seatIncumbencyMnisId ?seatIncumbencyMnisId;
                parl:parliamentaryIncumbencyHasMember ?member;
                parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat;
                parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod;
        		parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate;
                parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate.
        }
        where {
            bind(@subject as ?seatIncumbency)
            ?seatIncumbency parl:seatIncumbencyMnisId ?seatIncumbencyMnisId.
            optional {?seatIncumbency parl:parliamentaryIncumbencyHasMember ?member}
            optional {?seatIncumbency parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate}
            optional {?seatIncumbency parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate}
            optional {?seatIncumbency parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat}
            optional {?seatIncumbency parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod}            
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=MemberConstituency_Id,Member_Id,Constituency_Id,StartDate,EndDate";
        }
    }
}
