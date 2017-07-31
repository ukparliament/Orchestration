namespace Functions.TransformationCountry
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationCountry";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/json";
            }
        }

		public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Country.
            }
            where{
                ?s parl:countryGovRegisterId @countryGovRegisterId.
            }";
            }
        }

		public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?country a parl:Country;
                parl:countryName ?countryName;
                parl:countryOfficialName ?countryOfficialName;
                parl:countryCitizenNames ?countryCitizenNames;
                parl:govRegisterCountryStartDate ?govRegisterCountryStartDate;
                parl:govRegisterCountryEndDate ?govRegisterCountryEndDate;
                parl:countryGovRegisterId ?countryGovRegisterId.
        }
        where {
            bind(@subject as ?country)
        	?country parl:countryGovRegisterId ?countryGovRegisterId.
            optional {?country parl:countryName ?countryName}
            optional {?country parl:countryOfficialName ?countryOfficialName}
            optional {?country parl:countryCitizenNames ?countryCitizenNames}
            optional {?country parl:govRegisterCountryStartDate ?govRegisterCountryStartDate}
            optional {?country parl:govRegisterCountryEndDate ?govRegisterCountryEndDate}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://country.register.gov.uk/record/{dataUrl}";
        }
    }
}
