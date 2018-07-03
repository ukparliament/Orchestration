using Newtonsoft.Json;
using System;

namespace Functions
{
    public class BaseTransformationJsonMappingModel<T, K> : BaseTransformation<T, K>
        where T : ITransformationSettings, new()
        where K : BaseMappingModel

    {
        public override K GetSource(string dataUrl, T settings)
        {
            string url = settings.ParameterizedString(dataUrl);
            string response = DataRetrieval.DataFromUrl(url, settings.AcceptHeader, logger).Result;
            if (string.IsNullOrWhiteSpace(response))
                return null;

            K source = null;
            try
            {
                source = JsonConvert.DeserializeObject<K>(response);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
            }
            return source;
        }
    }
}
