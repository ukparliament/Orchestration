using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions.TransformationWebLink
{
    public static class TransformationWebLink
    {
        [FunctionName("TransformationWebLink")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            Transformation transformation = new Transformation();
            return await transformation.Run(req, new Settings(), executionContext);
        }
    }
}
