using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationCountryGovRegister
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationCountryGovRegister";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/json";
            }
        }

		public XmlNamespaceManager SourceXmlNamespaceManager
		{
			get
			{
				XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
				return sourceXmlNamespaceManager;
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
                ?s a parl:Country; 
                    parl:countryGovRegisterId @countryGovRegisterId.
            }";
            }
        }

		private Dictionary<string, string> subjectRetrievalParameters;
		public Dictionary<string, string> SubjectRetrievalParameters
		{
			get
			{
				return new Dictionary<string, string>
				{
					{"countryGovRegisterId","root/*/key" }
				};
			}
		}

		public string ExisitngGraphSparqlCommand
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
        	?country a parl:Country.
            optional {?country parl:countryName ?countryName}
            optional {?country parl:countryOfficialName ?countryOfficialName}
            optional {?country parl:countryCitizenNames ?countryCitizenNames}
            optional {?country parl:govRegisterCountryStartDate ?govRegisterCountryStartDate}
            optional {?country parl:govRegisterCountryEndDate ?govRegisterCountryEndDate}
            optional {?country parl:countryGovRegisterId ?countryGovRegisterId}
        }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return null;
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://country.register.gov.uk/record/{dataUrl}";
        }
    }
}
