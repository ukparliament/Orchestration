using System;

namespace Functions.TransformationDepartmentGovernmentOrganisation
{
    public class Settings :ITransformationSettings
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
        	?department a parl:GovRegisterGovernmentOrganisation;
                parl:governmentOrganisationGovRegisterId ?governmentOrganisationGovRegisterId.
        }
        where {
            bind(@subject as ?department)
        	?department parl:governmentOrganisationGovRegisterId ?governmentOrganisationGovRegisterId.
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "9855a9e1-54e1-431d-b6dd-bf0455d1b244").Replace("{id}", dataUrl);
        }
    }
}
