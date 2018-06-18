using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationProcedureBusinessItem
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            BusinessItem businessItem = new BusinessItem();
            Laying laying = new Laying();
            JObject jsonResponse = (JObject)JsonConvert.DeserializeObject(response);

            Uri idUri = giveMeUri(jsonResponse, "TripleStoreId");
            if (idUri == null)
                return null;
            else
            {
                businessItem.Id = idUri;
                laying.Id = idUri;
            }

            DateTimeOffset? dateTime = ((JValue)jsonResponse.SelectToken("Businessitem_x0020_date")).GetDate();
            if (dateTime.HasValue)
                businessItem.BusinessItemDate = new DateTimeOffset[] { dateTime.Value };
            businessItem.BusinessItemHasWorkPackage = giveMeUris(jsonResponse, "BelongsTo_x003a_TripleStoreId")
                .Select(u => new WorkPackage() { Id = u })
                .ToArray();
            businessItem.BusinessItemHasProcedureStep = giveMeUris(jsonResponse, "ActualisesProcedureStep_x003a_Tr")
                .Select(u => new ProcedureStep() { Id = u })
                .ToArray();
            string url = ((JValue)jsonResponse.SelectToken("Weblink")).GetText();
            if ((string.IsNullOrWhiteSpace(url) == false) &&
                (Uri.TryCreate(url, UriKind.Absolute, out Uri uri)))
                businessItem.BusinessItemHasBusinessItemWebLink = new List<BusinessItemWebLink>()
                {
                    new BusinessItemWebLink()
                    {
                        Id=uri
                    }
                };            
            Uri answeringBodyUri = giveMeUri(jsonResponse, "LayingBody_x003a_TripleStoreId.Value");
            if (answeringBodyUri != null)
            {
                laying.LayingHasLayingBody = new LayingBody()
                {
                    Id = answeringBodyUri
                };
                laying.LayingHasLayableThing = giveMeUris(jsonResponse, "BelongsTo_x003a_WorkPacakageable")
                    .Select(u => new LayableThing() { Id = u })
                    .SingleOrDefault();
            }
            return new BaseResource[] { businessItem, laying };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<BusinessItem>()
                .FirstOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

        private Uri giveMeUri(JObject jsonResponse, string tokenName)
        {
            string id = ((JValue)jsonResponse.SelectToken(tokenName)).GetText();
            if (string.IsNullOrWhiteSpace(id))
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

        private IEnumerable<Uri> giveMeUris(JObject jsonResponse, string tokenName)
        {
            foreach (JObject item in (JArray)jsonResponse.SelectToken(tokenName))
            {
                string itemId = item["Value"].ToString();
                if (string.IsNullOrWhiteSpace(itemId))
                    continue;
                if (Uri.TryCreate($"{idNamespace}{itemId}", UriKind.Absolute, out Uri itemUri))
                    yield return itemUri;
            }
        }
    }
}
