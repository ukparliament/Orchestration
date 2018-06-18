using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationDepartmentGovernmentOrganisation
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            GovRegisterGovernmentOrganisation department = new GovRegisterGovernmentOrganisation();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("GovRegisterID")).GetText();
            department.GovernmentOrganisationGovRegisterId = id;
            int? mnisId = ((JValue)jsonResponse.SelectToken("mnisID")).GetNumber();
            if (mnisId.HasValue)
            {
                Uri departmentUri = IdRetrieval.GetSubject("mnisDepartmentId", mnisId.ToString(), false, logger);
                if (departmentUri == null)
                    return null;
                else
                    department.Id = departmentUri;
            }
            else
                return null;
            return new BaseResource[] { department };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<GovRegisterGovernmentOrganisation>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }
    }
}
