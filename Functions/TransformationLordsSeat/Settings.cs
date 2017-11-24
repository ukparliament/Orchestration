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
                return @"
            construct{
                ?s a parl:HouseSeat.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:houseSeatName ?houseSeatName.
            }";
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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_LordsSeatItem", EnvironmentVariableTarget.Process).Replace("{id}", dataUrl);
        }
    }
}
