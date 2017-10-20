using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.TransformationHouseIncumbencyMnis
{
    public static class TransformationHouseIncumbencyMnis
    {
        [FunctionName("TransformationHouseIncumbencyMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            Transformation transformation = new Transformation();
            return await transformation.Run(req, new Settings(), executionContext);
        }
    }
}