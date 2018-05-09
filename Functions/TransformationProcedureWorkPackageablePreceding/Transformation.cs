using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf;
using System;
using System.Linq;

namespace Functions.TransformationProcedureWorkPackageablePreceding
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            IWorkPackageableThing workPackageableThing = new WorkPackageableThing();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            Uri idUri = giveMeUri(jsonResponse, "FollowingWorkPackageableThing_x0");
            if (idUri == null)
                return null;
            else
                workPackageableThing.Id = idUri;
            Uri precedingUri= giveMeUri(jsonResponse, "PrecedingWorkPackageableThing_x0");
            if (precedingUri == null)
                return null;
            else
                workPackageableThing.WorkPackageableThingPrecedesWorkPackageableThing = new IWorkPackageableThing[] { new WorkPackageableThing() { Id = precedingUri } };

            return new IResource[] { workPackageableThing };
        }

        public override Uri GetSubjectFromSource(IResource[] deserializedSource)
        {
            return deserializedSource.OfType<IWorkPackageableThing>()
                .SingleOrDefault()
                .Id;
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            return source;
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            string id = ((JValue)jsonResponse.SelectToken(tokenName)?.SelectToken("Value"))?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                logger.Warning("No Id info found");
                return null;
            }
            else
            {
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri idUri))
                    return idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
        }
    }
}
