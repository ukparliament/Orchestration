using System;

namespace Functions.TransformationHouseSeatType
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
            ?houseSeatType a parl:HouseSeatType;
        	    parl:houseSeatTypeName ?houseSeatTypeName;
                parl:houseSeatTypeMnisId ?houseSeatTypeMnisId;
                parl:houseSeatTypeDescription ?houseSeatTypeDescription.
        }
        where {
            bind(@subject as ?houseSeatType)
            ?houseSeatType parl:houseSeatTypeName ?houseSeatTypeName.
            optional {?houseSeatType parl:houseSeatTypeMnisId ?houseSeatTypeMnisId}
            optional {?houseSeatType parl:houseSeatTypeDescription ?houseSeatTypeDescription}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "85fe54cd-d26a-43af-88a5-9063f747c4a9").Replace("{id}", dataUrl);
        }
    }
}
