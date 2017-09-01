using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.TransformationCommitteeMnis
{
    public static class TransformationCommitteeMnis
    {
        [FunctionName("TransformationCommitteeMnis")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            Transformation transformation = new Transformation();
            return await transformation.Run(req, new Settings());
        }
    }
}