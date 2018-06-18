using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;

namespace Functions.TransformationLordsSeatIncumbencyInterruption
{
    public class Transformation : BaseTransformation<Settings>
    {
        
        public override BaseResource[] TransformSource(string response)
        {
            IncumbencyInterruption incumbencyInterruption = new IncumbencyInterruption();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

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


            PastIncumbencyInterruption pastInterruption = new PastIncumbencyInterruption();
            pastInterruption.Id = incumbencyInterruption.Id;
            pastInterruption.IncumbencyInterruptionEndDate= ((JValue)jsonResponse.SelectToken("endDate")).GetDate();

            return new BaseResource[] { incumbencyInterruption, pastInterruption };
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
