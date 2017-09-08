using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.QueueMessagesRetrieval
{
    public static class QueueMessagesRetrieval
    {
        private static Logger logger;

        private static readonly string serviceBusEndpoint = System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_ServiceBus", EnvironmentVariableTarget.Process);

        [FunctionName("QueueMessagesRetrieval")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log, ExecutionContext executionContext)
        {
            logger = new Logger(executionContext);
            logger.Triggered();

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
                logger.Error("Missing some value(s)");
                logger.Finished();
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            logger.SetQueueName(queue.ToString());
            logger.Verbose($"Queue: {queue}, Batch size: {batchSize}");
            int totalNumber = 0;
            try
            {
                logger.Verbose("Getting number");
                NamespaceManager manager = NamespaceManager.CreateFromConnectionString(serviceBusEndpoint);
                totalNumber = (int)manager.GetQueue(queue.ToString()).MessageCount;
                logger.Metric("RemainingMessageCount", totalNumber);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                logger.Error("Problem with getting number");
                logger.Finished();
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Problem with getting number");
            }
            if (totalNumber == 0)
            {
                logger.Verbose("No messages");
                logger.Finished();
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            Microsoft.ServiceBus.Messaging.QueueClient client = null;
            try
            {
                logger.Verbose("Connecting");
                client = getQueue(serviceBusEndpoint, queue);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                logger.Error("Problem with connection");
                logger.Finished();
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
                logger.Exception(e);
                logger.Error("Problem with messages");
                logger.Finished();
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
                    logger.Exception(e);
                    logger.Error($"Problem with messages ({batchSize}/{result.Count()})");
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
            logger.Finished();
            logger.MessageCount(totalNumber, result.Count());

            return req.CreateResponse<MessageItem[]>(HttpStatusCode.OK, result.ToArray());
        }

        private static async Task<IEnumerable<MessageItem>> getMessages(Microsoft.ServiceBus.Messaging.QueueClient client, int batchSize)
        {
            IEnumerable<Microsoft.ServiceBus.Messaging.BrokeredMessage> messages = await client.ReceiveBatchAsync(batchSize);
            logger.Metric("FetchedMessageCount", messages.Count());

            return messages.Select(m => new MessageItem().ConvertMe(m));
        }

        private static Microsoft.ServiceBus.Messaging.QueueClient getQueue(string connectionString, string queueName)
        {
            return Microsoft.ServiceBus.Messaging.QueueClient.CreateFromConnectionString(connectionString, queueName, Microsoft.ServiceBus.Messaging.ReceiveMode.ReceiveAndDelete);
        }

    }
}