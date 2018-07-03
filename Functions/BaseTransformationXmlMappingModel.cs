using System;
using System.IO;
using System.Xml.Serialization;

namespace Functions
{
    public class BaseTransformationXmlMappingModel<T, K> : BaseTransformation<T, K>
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
                using (StringReader reader = new StringReader(response))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(K));
                    source = (K)serializer.Deserialize(reader);
                }
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
