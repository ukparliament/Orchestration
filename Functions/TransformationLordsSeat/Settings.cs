using System;

namespace Functions.TransformationLordsSeat
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
            ?houseSeat a parl:HouseSeat;
        	    parl:houseSeatName ?houseSeatName;
                parl:houseSeatHasHouseSeatType ?houseSeatHasHouseSeatType;
                parl:houseSeatHasHouse ?houseSeatHasHouse.
        }
        where {
            bind(@subject as ?houseSeat)
            ?houseSeat parl:houseSeatName ?houseSeatName.
            optional {?houseSeat parl:houseSeatHasHouse ?houseSeatHasHouse}
            optional {?houseSeat parl:houseSeatHasHouseSeatType ?houseSeatHasHouseSeatType}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "37847503-9069-467b-be45-ffed7c405863").Replace("{id}", dataUrl);
        }
    }
}
