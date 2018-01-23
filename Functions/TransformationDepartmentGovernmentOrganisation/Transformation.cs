using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationDepartmentGovernmentOrganisation
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
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
                    department.SubjectUri = departmentUri;
            }
            else
                return null;
            return new IOntologyInstance[] { department };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IGovRegisterGovernmentOrganisation>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            IGovRegisterGovernmentOrganisation department = source.OfType<IGovRegisterGovernmentOrganisation>().SingleOrDefault();
            
            return new IOntologyInstance[] { department };
        }
    }
}
