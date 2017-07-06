using Microsoft.ApplicationInsights;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Functions
{
    public static class XmlDataRetrieval
    {

        public static async Task<XDocument> GetXmlDataFromUrl(string url, string acceptHeader, Logger logger)
        {
            XDocument doc;
            string xml = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                logger.Verbose("Contacting source");
                xml = await getXmlText(url, acceptHeader);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                logger.Exception(e);
                return null;
            }
            finally
            {
                externalTimer.Stop();
                logger.Dependency("XmlDataRetrieval", url, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (string.IsNullOrWhiteSpace(xml))
            {
                logger.Error("Empty response");
                return null;
            }
            try
            {
                logger.Verbose("Parsing response");
                doc = XDocument.Parse(xml);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
            }
            return doc;
        }

        private static async Task<string> getXmlText(string url, string acceptHeader)
        {
            string xml = null;
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(180);
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (string.IsNullOrWhiteSpace(acceptHeader) == false)
                        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode == true)
                        {
                            if ((acceptHeader != null) && (acceptHeader.Contains("json")))
                            {
                                string json = await response.Content.ReadAsStringAsync();
                                xml = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, "root").InnerXml;
                            }
                            else
                                xml = await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }

            return xml;
        }

    }
}
