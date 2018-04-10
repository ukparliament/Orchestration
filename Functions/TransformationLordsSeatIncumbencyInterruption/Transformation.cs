using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query;

namespace Functions.TransformationLordsSeatIncumbencyInterruption
{
    public class Transformation : BaseTransformation<Settings>
    {
        
        public override IResource[] TransformSource(string response)
        {
            IIncumbencyInterruption incumbencyInterruption = new IncumbencyInterruption();
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
                incumbencyInterruption.IncumbencyInterruptionHasIncumbency = new SeatIncumbency()
                {
                    Id = incumbencyUri
                };

            incumbencyInterruption.IncumbencyInterruptionStartDate = ((JValue)jsonResponse.SelectToken("startDate")).GetDate();


            IPastIncumbencyInterruption pastInterruption = new PastIncumbencyInterruption();
            pastInterruption.Id = incumbencyInterruption.Id;
            pastInterruption.IncumbencyInterruptionEndDate= ((JValue)jsonResponse.SelectToken("endDate")).GetDate();

            return new IResource[] { incumbencyInterruption, pastInterruption };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IIncumbencyInterruption>()
                .FirstOrDefault()
                .Id;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IIncumbencyInterruption[] interruptions = source.OfType<IIncumbencyInterruption>().ToArray();

            return interruptions as IResource[];
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
