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
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "fa0d3696-92be-4506-93c9-13f5e9dd6b80").Replace("{id}", dataUrl);
        }
    }
}
