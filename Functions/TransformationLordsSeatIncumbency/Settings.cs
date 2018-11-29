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
                return null;
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
                parl:lordsSeatIncumbencyMnisId ?lordsSeatIncumbencyMnisId.
        }
        where {
            bind(@subject as ?seatIncumbency)
            ?seatIncumbency parl:seatIncumbencyHasHouseSeat ?seatIncumbencyHasHouseSeat.
            optional {?seatIncumbency parl:parliamentaryIncumbencyHasMember ?parliamentaryIncumbencyHasMember}
            optional {?seatIncumbency parl:parliamentaryIncumbencyStartDate ?parliamentaryIncumbencyStartDate}
            optional {?seatIncumbency parl:parliamentaryIncumbencyEndDate ?parliamentaryIncumbencyEndDate}
            optional {?seatIncumbency parl:seatIncumbencyHasParliamentPeriod ?seatIncumbencyHasParliamentPeriod}
            optional {?seatIncumbency parl:lordsSeatIncumbencyMnisId ?lordsSeatIncumbencyMnisId}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "5c1600fd-34ed-4227-b6f8-dcb03ed6d8be").Replace("{id}", dataUrl);
        }
    }
}
