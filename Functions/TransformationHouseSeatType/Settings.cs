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
                return @"
            construct{
                ?s a parl:HouseSeatType.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:houseSeatTypeName ?houseSeatTypeName.
            }";
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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_HouseSeatTypeItem", EnvironmentVariableTarget.Process).Replace("{id}", dataUrl);
        }
    }
}
