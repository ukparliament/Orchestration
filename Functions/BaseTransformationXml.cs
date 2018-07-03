using System;
using System.Xml.Linq;

namespace Functions
{
    public class BaseTransformationXml<T, K> : BaseTransformation<T, K>
        where T : ITransformationSettings, new()
        where K : XDocument

    {
        protected XNamespace atom = "http://www.w3.org/2005/Atom";
        protected XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        protected XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override K GetSource(string dataUrl, T settings)
        {
            string url = settings.ParameterizedString(dataUrl);
            string response = DataRetrieval.DataFromUrl(url, settings.AcceptHeader, logger).Result;
            if (string.IsNullOrWhiteSpace(response))
                return null;
            XDocument doc = null;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
            }
            return doc as K;
        }
    }
}
