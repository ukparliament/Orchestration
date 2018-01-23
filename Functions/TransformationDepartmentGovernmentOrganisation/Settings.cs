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
                return @"
            construct{
                ?s a parl:GovRegisterGovernmentOrganisation.
            }
            where{
                bind(@subjectUri as ?s)
                ?s parl:mnisDepartmentId ?mnisDepartmentId.
            }";
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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "a55100f4-8dd9-43a3-8614-3a3f121c8fbf").Replace("{id}", dataUrl);
        }
    }
}
