using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationLordsSeatIncumbencyInterruption
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {

        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            IncumbencyInterruption incumbencyInterruption = new IncumbencyInterruption();
            string id = ((JValue)jsonResponse.SelectToken("tripleStoreId")).GetText();
            Uri uri = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out uri))
                    incumbencyInterruption.Id = uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            Uri incumbencyUri = giveMeUri(jsonResponse, "incumbencyId");
            if (incumbencyUri == null)
                return null;
            else
                incumbencyInterruption.IncumbencyInterruptionHasIncumbency = new Incumbency()
                {
                    Id = incumbencyUri
                };

            incumbencyInterruption.IncumbencyInterruptionStartDate = ((JValue)jsonResponse.SelectToken("startDate")).GetDate();
            incumbencyInterruption.IncumbencyInterruptionEndDate = ((JValue)jsonResponse.SelectToken("endDate")).GetDate();

            return new BaseResource[] { incumbencyInterruption };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<IncumbencyInterruption>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            IncumbencyInterruption[] interruptions = source.OfType<IncumbencyInterruption>().ToArray();

            return interruptions as BaseResource[];
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            object id = ((JValue)jsonResponse.SelectToken($"{tokenName}"))?.Value;
            if (id == null)
            {
                logger.Warning($"No {tokenName} Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }
    }
}
