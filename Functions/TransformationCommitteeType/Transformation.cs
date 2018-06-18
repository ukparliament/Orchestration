using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationCommitteeType
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            FormalBodyType formalBodyType = new FormalBodyType();
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
                    formalBodyType.Id = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            formalBodyType.FormalBodyTypeName= ((JValue)jsonResponse.SelectToken("Title")).GetText();

            return new BaseResource[] { formalBodyType };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<FormalBodyType>()
                .SingleOrDefault()
                .Id;            
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            return source;
        }

    }
}
