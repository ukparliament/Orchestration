using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationCommitteeType
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IOntologyInstance[] TransformSource(string response)
        {
            IFormalBodyType formalBodyType = new FormalBodyType();
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
                    formalBodyType.SubjectUri = idUri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
            }
            formalBodyType.FormalBodyTypeName= ((JValue)jsonResponse.SelectToken("Title")).GetText();

            return new IOntologyInstance[] { formalBodyType };
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            Uri subjectUri = deserializedSource.OfType<IFormalBodyType>()
                .SingleOrDefault()
                .SubjectUri;
            return new Dictionary<string, object>()
            {
                { "subjectUri", subjectUri }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            return source.OfType<IFormalBodyType>().ToArray();
        }

    }
}
