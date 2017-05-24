using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions
{
    public static class XmlDataRetrieval
    {

        public static async Task<string> GetXmlDataFromUrl(string url, string acceptHeader)
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
