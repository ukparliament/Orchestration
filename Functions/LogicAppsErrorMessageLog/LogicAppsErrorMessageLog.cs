using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Functions.LogicAppsErrorMessageLog
{
    public static class LogicAppsErrorMessageLog
    {
        private static Logger logger;

        [FunctionName("LogicAppsErrorMessageLog")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            logger.SetOperationName("LogicAppsErrorMessageLog");
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