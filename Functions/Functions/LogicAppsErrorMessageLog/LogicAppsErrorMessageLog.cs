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
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = System.Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };

        [FunctionName("LogicAppsErrorMessageLog")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "LogicAppsErrorMessageLog";
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();

            telemetryClient.TrackEvent("Triggered");
            string json = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            dynamic action = data?.action;
            dynamic workflow = data?.workflow;

            if ((action == null) || (workflow == null))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");

            if (data.batchId != null)
                telemetryClient.Context.Properties["BatchId"] = data.batchId.ToString();

            Dictionary<string, string> properties = new Dictionary<string, string>()
                {
                    {"actionName",action?.name?.ToString() },
                    {"startTime",action?.startTime?.ToString() },
                    {"workflowRunId",workflow?.run?.id?.ToString() },
                    {"message", (action?.error?.message??action?.body?.Message??action?.outputs?.body)?.ToString()}
                };
            telemetryClient.TrackTrace(workflow?.name?.ToString() ?? "Logic App error", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error, properties);

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}