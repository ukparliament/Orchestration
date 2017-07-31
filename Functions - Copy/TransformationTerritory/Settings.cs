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

		public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Territory.
            }
            where{
                ?s parl:territoryGovRegisterId @territoryGovRegisterId.
            }";
            }
        }

		public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?territory parl:territoryName ?territoryName;
                parl:territoryOfficialName ?territoryOfficialName;
                parl:govRegisterTerritoryStartDate ?govRegisterTerritoryStartDate;
                parl:govRegisterTerritoryEndDate ?govRegisterTerritoryEndDate;
                parl:territoryGovRegisterId ?territoryGovRegisterId.
        }
        where {
            bind(@subject as ?territory)
            ?territory parl:territoryGovRegisterId ?territoryGovRegisterId.
            optional {?territory parl:territoryName ?territoryName}
            optional {?territory parl:territoryOfficialName ?territoryOfficialName}
            optional {?territory parl:govRegisterTerritoryStartDate ?govRegisterTerritoryStartDate}
            optional {?territory parl:govRegisterTerritoryEndDate ?govRegisterTerritoryEndDate}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"https://territory.register.gov.uk/record/{dataUrl}";
        }
    }
}
