using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Linq;

namespace Functions.TransformationProcedureStep
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IProcedureStep procedureStep = new ProcedureStep();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("TripleStoreId")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    procedureStep.Id = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            procedureStep.ProcedureStepName = ((JValue)jsonResponse.SelectToken("Title")).GetText();
            procedureStep.ProcedureStepDescription = ((JValue)jsonResponse.SelectToken("Description")).GetText();

            return new IResource[] { procedureStep };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IProcedureStep>()
                .SingleOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IProcedureStep procedureStep = source.OfType<IProcedureStep>().SingleOrDefault();

            return new IResource[] { procedureStep };
        }
    }
}
