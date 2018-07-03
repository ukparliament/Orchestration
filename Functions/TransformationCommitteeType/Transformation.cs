using Newtonsoft.Json.Linq;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Linq;

namespace Functions.TransformationCommitteeType
{
    public class Transformation : BaseTransformationJson<Settings, JObject>
    {
        public override BaseResource[] TransformSource(JObject jsonResponse)
        {
            FormalBodyType formalBodyType = new FormalBodyType();
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
            formalBodyType.FormalBodyTypeName = ((JValue)jsonResponse.SelectToken("Title")).GetText();

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
