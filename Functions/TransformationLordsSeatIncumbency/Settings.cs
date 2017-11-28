using System;

namespace Functions.TransformationLordsSeatIncumbency
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
                ?s a parl:SeatIncumbency.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?seatIncumbency a parl:SeatIncumbency;
        	    parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat;
                parl:parliamentaryIncumbencyHasMember ?parliamentaryIncumbencyHasMember;
                parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate;
                parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate;
                parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod;
                parl:seatIncumbencyMnisId ?seatIncumbencyMnisId.
        }
        where {
            bind(@subject as ?seatIncumbency)
            ?seatIncumbency parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat.
            optional {?seatIncumbency parl:parliamentaryIncumbencyHasMember ?parliamentaryIncumbencyHasMember}
            optional {?seatIncumbency parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate}
            optional {?seatIncumbency parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate}
            optional {?seatIncumbency parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod}
            optional {?seatIncumbency parl:seatIncumbencyMnisId ?seatIncumbencyMnisId}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_LordsSeatIncumbencyItem", EnvironmentVariableTarget.Process).Replace("{listId}", "a9c47d77-9ccb-44cc-bc90-c3d07ea8aa0e").Replace("{id}", dataUrl);
        }
    }
}
