using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackageableThing
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IWorkPackageableThing workPackageableThing = new WorkPackageableThing();
            IStatutoryInstrument statutoryInstrument = new StatutoryInstrument();            
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            string id = ((JValue)jsonResponse.SelectToken("WorkPacakageableThingTripleStore")).GetText();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                {
                    workPackageableThing.Id = idUri;
                    statutoryInstrument.Id = idUri;
                }
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            workPackageableThing.WorkPackageableThingName = ((JValue)jsonResponse.SelectToken("Title")).GetText();
            workPackageableThing.WorkPackageableThingComingIntoForceNote = ((JValue)jsonResponse.SelectToken("ComingIntoForceNote")).GetText();
            workPackageableThing.WorkPackageableThingComingIntoForceDate = ((JValue)jsonResponse.SelectToken("ComingIntoForceDate")).GetDate();
            workPackageableThing.WorkPackageableThingTimeLimitForObjectionEndDate = ((JValue)jsonResponse.SelectToken("ObjectionDeadline")).GetDate();
            string url = ((JValue)jsonResponse.SelectToken("WorkPackageableThingUrl")).GetText();
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                workPackageableThing.WorkPackageableThingHasWorkPackageableThingWebLink = new List<IWorkPackageableThingWebLink>()
                {
                    new WorkPackageableThingWebLink()
                    {
                        Id=uri
                    }
                };
            
            statutoryInstrument.StatutoryInstrumentNumber = ((JValue)jsonResponse.SelectToken("SINumber")).GetText();
            return new IResource[] { workPackageableThing, statutoryInstrument };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IWorkPackageableThing>()
                .FirstOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            return source;
        }
    }
}
