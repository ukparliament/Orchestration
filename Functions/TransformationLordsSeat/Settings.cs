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
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "4a000e4a-aa78-4338-806d-92c0dd42e1a6").Replace("{id}", dataUrl);
        }
    }
}
