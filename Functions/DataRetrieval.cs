using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions
{
    public static class DataRetrieval
    {
        private static HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(180)
        };

        public static async Task<string> DataFromUrl(string url, string acceptHeader, Logger logger)
        {
            string result = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;

            try
            {
                logger.Verbose("Contacting source");
                result = await getText(url, acceptHeader);
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
                logger.Dependency("DataRetrieval", url, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                logger.Error("Empty response");
                return null;
            }
            return result;
        }

        private static async Task<string> getText(string url, string acceptHeader)
        {
            string result = null;
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (string.IsNullOrWhiteSpace(acceptHeader) == false)
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptHeader));
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode == true)
                        result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }

    }
}
