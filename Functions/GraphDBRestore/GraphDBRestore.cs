using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Functions.GraphDBRestore
{
    public static class GraphDBRestore
    {
        private static Logger logger;

        private static readonly string releaseId = Environment.GetEnvironmentVariable("ReleaseId", EnvironmentVariableTarget.Process);
        private static readonly string storageAccountConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_BackupStorage", EnvironmentVariableTarget.Process);
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);
        private static readonly string apiVersion = Environment.GetEnvironmentVariable("ApiVersion", EnvironmentVariableTarget.Process);

        [FunctionName("GraphDBRestore")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log, Microsoft.Azure.WebJobs.ExecutionContext executionContext)
        {
            logger = new Logger(executionContext);
            logger.Triggered();
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);
            if (data.backupUrl == null)
            {
                logger.Error("Missing backup url");
                logger.Finished();
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing backup url");
            }
            new Thread(() => restoreBackup(data.backupUrl.ToString())).Start();
            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        private static void finishTelemetry(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage) == false)
                logger.Error(errorMessage);
            logger.Finished();
        }

        private static async Task restoreBackup(string backupUrl)
        {
            bool externalCallOk = true;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            try
            {
                await replaceDataWithBackup(backupUrl);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                logger.Exception(e);
                finishTelemetry(e.Message);
            }
            finally
            {
                externalTimer.Stop();
                logger.Dependency(backupUrl, dataAPI, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            finishTelemetry(null);
        }

        private static async Task replaceDataWithBackup(string backupUrl)
        {
            string graphDBRestoreUrl = $"{dataAPI}/repositories/Master/statements?context=null";

            using (HttpClient backupClient = new HttpClient())
            {
                using (Stream backupStream = await backupClient.GetStreamAsync(backupUrl))
                {
                    using (HttpClient restoreClient = new HttpClient())
                    {
                        restoreClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                        restoreClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                        restoreClient.DefaultRequestHeaders.Add("Api-Version", apiVersion);
                        StreamContent restoreContent = new StreamContent(backupStream);
                        restoreContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-trig");
                        using (HttpResponseMessage response = await restoreClient.PutAsync(graphDBRestoreUrl, restoreContent))
                        {
                            if (response.IsSuccessStatusCode == false)
                                logger.Warning($"Status code {response.StatusCode}");
                        }
                    }
                }
            }
        }
    }
}