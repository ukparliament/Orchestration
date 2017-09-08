using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.LogicAppsErrorMessageLog
{
    public static class LogicAppsErrorMessageLog
    {
        [FunctionName("LogicAppsErrorMessageLog")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            Logger logger = new Logger(executionContext);
            logger.Triggered();
            string json = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            dynamic action = data?.action;
            dynamic workflow = data?.workflow;

            if ((action == null) || (workflow == null))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");

            if (data.batchId != null)
                logger.SetBatchId(data.batchId.ToString());

            Dictionary<string, string> properties = new Dictionary<string, string>()
                {
                    {"actionName",action?.name?.ToString() },
                    {"startTime",action?.startTime?.ToString() },
                    {"workflowRunId",workflow?.run?.id?.ToString() },
                    {"message", (action?.error?.message??action?.body?.Message??action?.outputs?.body)?.ToString()}
                };
            logger.Error(workflow?.name?.ToString() ?? "Logic App error", properties);

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}