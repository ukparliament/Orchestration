using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationDepartmentGovernmentOrganisation
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IGovRegisterGovernmentOrganisation department = new GovRegisterGovernmentOrganisation();            
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("GovRegisterID")).GetText();
            department.GovernmentOrganisationGovRegisterId = id;
            int? mnisId = ((JValue)jsonResponse.SelectToken("mnisID")).GetFloat();
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
            return new IResource[] { department };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IGovRegisterGovernmentOrganisation>()
                .SingleOrDefault()
                .Id;            
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IGovRegisterGovernmentOrganisation department = source.OfType<IGovRegisterGovernmentOrganisation>().SingleOrDefault();
            
            return new IResource[] { department };
        }
    }
}
