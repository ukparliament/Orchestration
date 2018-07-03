using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Functions
{
    public class BaseTransformationJson<T, K> : BaseTransformation<T, K>
        where T : ITransformationSettings, new()
        where K : JObject

    {
        public override K GetSource(string dataUrl, T settings)
        {
            string url = settings.ParameterizedString(dataUrl);
            string response = DataRetrieval.DataFromUrl(url, settings.AcceptHeader, logger).Result;
            if (string.IsNullOrWhiteSpace(response))
                return null;
            JObject json = null;
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(response);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
            }
            return json as K;
        }
    }
}
