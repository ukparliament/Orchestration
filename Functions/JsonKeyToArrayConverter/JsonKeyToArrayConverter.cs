using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Functions.JsonKeyToArrayConverter
{
    public class JsonKeyToArrayConverter
    {
        protected static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        [FunctionName("JsonKeyToArrayConverter")]

        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if (data.url == null)
            {
                telemetryClient.TrackTrace("Missing some value(s)", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            string jsonText = await getData(data.url.ToString());
            return jsonKeyToArrayConversion(jsonText);
        }
        private static async Task<string> getData(string url)
        {
            string data = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                data = await client.GetStringAsync(url);
            }
            return data;
        }

        private static IEnumerable<string> jsonKeyToArrayConversion(string jsonText)
        {
            JObject json = JObject.Parse(jsonText);
            return json.Properties().Select(p => p.Name);
        }
    }
}