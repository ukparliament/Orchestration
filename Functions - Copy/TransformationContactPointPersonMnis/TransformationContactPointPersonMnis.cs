using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Functions.TransformationContactPointPersonMnis
{
    public static class TransformationContactPointPersonMnis
    {
        [FunctionName("TransformationContactPointPersonMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            Transformation transformation = new Transformation();
            return await transformation.Run(req, new Settings());
        }
    }
}