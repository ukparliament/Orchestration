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

        public static async Task<XDocument> GetXmlDataFromUrl(string url, string acceptHeader, TelemetryClient telemetryClient)
        {
            XDocument doc;
            string xml = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                telemetryClient.TrackTrace("Contacting source", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                xml = await getXmlText(url, acceptHeader);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
                return null;
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("XmlDataRetrieval", url, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (string.IsNullOrWhiteSpace(xml))
            {
                telemetryClient.TrackTrace("Empty response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                return null;
            }
            try
            {
                telemetryClient.TrackTrace("Parsing response", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                doc = XDocument.Parse(xml);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
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
