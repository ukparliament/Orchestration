using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Functions.JsonKeyToArrayConverter
{
    public class JsonKeyToArrayConverter
    {
        [FunctionName("JsonKeyToArrayConverter")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            Logger logger = new Logger(executionContext);
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if (data.url == null)
            {
                logger.Error("Missing some value(s)");
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