using System.Collections.Generic;
using System.Xml;

namespace Functions.TransformationTerritory
{
    public class Settings :ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationTerritory";
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
                ?s a parl:Territory.
            }
            where{
                ?s a parl:Territory; 
                    parl:territoryGovRegisterId @territoryGovRegisterId.
            }";
            }
        }

		public Dictionary<string, string> SubjectRetrievalParameters
		{
			get
			{
				return new Dictionary<string, string>
				{
					{"territoryGovRegisterId","root/*/key" }
				};
			}
		}

		public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?territory a parl:Territory;
                parl:territoryName ?territoryName;
                parl:territoryOfficialName ?territoryOfficialName;
                parl:govRegisterTerritoryStartDate ?govRegisterTerritoryStartDate;
                parl:govRegisterTerritoryEndDate ?govRegisterTerritoryEndDate;
                parl:territoryGovRegisterId ?territoryGovRegisterId.
        }
        where {
            bind(@subject as ?territory)
        	?territory a parl:Territory.
            optional {?territory parl:territoryName ?territoryName}
            optional {?territory parl:territoryOfficialName ?territoryOfficialName}
            optional {?territory parl:govRegisterTerritoryStartDate ?govRegisterTerritoryStartDate}
            optional {?territory parl:govRegisterTerritoryEndDate ?govRegisterTerritoryEndDate}
            optional {?territory parl:territoryGovRegisterId ?territoryGovRegisterId}
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
            return $"https://territory.register.gov.uk/record/{dataUrl}";
        }
    }
}
