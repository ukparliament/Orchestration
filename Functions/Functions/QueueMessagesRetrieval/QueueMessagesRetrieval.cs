using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.QueueMessagesRetrieval
{
    public static class QueueMessagesRetrieval
    {
        private static readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        private static readonly string serviceBusEndpoint = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_ServiceBus", EnvironmentVariableTarget.Process);

        [FunctionName("QueueMessagesRetrieval")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            telemetryClient.Context.Operation.Name = "QueueMessagesRetrieval";
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();

            telemetryClient.TrackEvent("Triggered");
            Stopwatch timer = Stopwatch.StartNew();

            string queue = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "queue", true) == 0)
                .Value;
            string batch = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "batchSize", true) == 0)
                .Value;

            dynamic data = await req.Content.ReadAsAsync<object>();

            queue = queue ?? data?.queue;
            batch = batch ?? data?.batchSize;
            int batchSize;
            if (int.TryParse(batch, out batchSize) == false)
                batchSize = 1;

            if ((string.IsNullOrEmpty(queue)) || (batchSize <= 0))
            {
                timer.Stop();
                telemetryClient.TrackTrace("Missing some value(s)", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            telemetryClient.Context.Properties["QueueName"] = queue.ToString();

            telemetryClient.TrackTrace($"Queue: {queue}, Batch size: {batchSize}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            int totalNumber = 0;
            try
            {
                telemetryClient.TrackTrace("Getting number", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                NamespaceManager manager = NamespaceManager.CreateFromConnectionString(serviceBusEndpoint);
                totalNumber = (int)manager.GetQueue(queue.ToString()).MessageCount;
                telemetryClient.TrackMetric("RemainingMessageCount", totalNumber);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                timer.Stop();
                telemetryClient.TrackEvent("Problem with getting number", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Problem with getting number");
            }
            if (totalNumber == 0)
            {
                telemetryClient.TrackTrace("No messages", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                timer.Stop();
                telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            Microsoft.ServiceBus.Messaging.QueueClient client = null;
            try
            {
                telemetryClient.TrackTrace("Connecting", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                client = getQueue(serviceBusEndpoint, queue);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                timer.Stop();
                telemetryClient.TrackEvent("Problem with connection", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Problem with connection");
            }

            List<MessageItem> result = new List<MessageItem>();
            IEnumerable<MessageItem> messages;
            try
            {
                messages = await getMessages(client, batchSize);
                result.AddRange(messages);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                timer.Stop();
                telemetryClient.TrackEvent("Problem with messages", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Problem with messages");
            }
            while ((result.Count() < batchSize) && (messages.Any()))
            {
                try
                {
                    messages = await getMessages(client, batchSize - result.Count());
                    result.AddRange(messages);
                }
                catch (Exception e)
                {
                    telemetryClient.TrackException(e);
                    telemetryClient.TrackTrace($"Problem with messages ({batchSize}/{result.Count()})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                    if (result.Any())
                    {
                        break;
                    }
                    else
                    {
                        return req.CreateResponse(HttpStatusCode.InternalServerError, $"Problem with messages ({batchSize}/{result.Count()})");
                    }
                }
            }
            timer.Stop();
            telemetryClient.TrackEvent("Finished", new Dictionary<string, string>(), new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds }, { "OriginalMessageCount", totalNumber }, { "RetrievedMessageCount", result.Count() } });

            return req.CreateResponse<MessageItem[]>(HttpStatusCode.OK, result.ToArray());
        }

        private static async Task<IEnumerable<MessageItem>> getMessages(Microsoft.ServiceBus.Messaging.QueueClient client, int batchSize)
        {
            IEnumerable<Microsoft.ServiceBus.Messaging.BrokeredMessage> messages = await client.ReceiveBatchAsync(batchSize);
            telemetryClient.TrackMetric("FetchedMessageCount", messages.Count());

            return messages.Select(m => new MessageItem().ConvertMe(m));
        }

        private static Microsoft.ServiceBus.Messaging.QueueClient getQueue(string connectionString, string queueName)
        {
            return Microsoft.ServiceBus.Messaging.QueueClient.CreateFromConnectionString(connectionString, queueName, Microsoft.ServiceBus.Messaging.ReceiveMode.ReceiveAndDelete);
        }

    }
}