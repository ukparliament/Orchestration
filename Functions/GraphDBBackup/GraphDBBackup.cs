using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Functions.GraphDBBackup
{
    public static class GraphDBBackup
    {
        private static Logger logger;

        private static readonly string backupContainer = Environment.GetEnvironmentVariable("BackupContainer", EnvironmentVariableTarget.Process);
        private static readonly string storageAccountConnectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_BackupStorage", EnvironmentVariableTarget.Process);
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);
        private static readonly string apiVersion = Environment.GetEnvironmentVariable("ApiVersion", EnvironmentVariableTarget.Process);

        [FunctionName("GraphDBBackup")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log, Microsoft.Azure.WebJobs.ExecutionContext executionContext)
        {
            logger = new Logger(executionContext);
            logger.Triggered();
            new Thread(() => createBackup()).Start();
            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        private static void finishTelemetry(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage) == false)
                logger.Error(errorMessage);
            logger.Finished();
        }

        private static async Task createBackup()
        {
            CloudBlockBlob blob = null;
            bool externalCallOk = true;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            try
            {
                blob = await establishBlobConnection();
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
                logger.Dependency("GraphDBBackupStorageConnection", storageAccountConnectionString, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            if (externalCallOk == true)
            {
                externalTimer = Stopwatch.StartNew();
                externalStartTime = DateTime.UtcNow;
                try
                {
                    await uploadBackup(blob);
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
                    logger.Dependency("GraphDBBackupStream", dataAPI, externalStartTime, externalTimer.Elapsed, externalCallOk);
                }
                finishTelemetry(null);
            }
        }

        private static async Task<CloudBlockBlob> establishBlobConnection()
        {
            //https://feedback.azure.com/forums/217298-storage/suggestions/2474308-provide-time-to-live-feature-for-blobs - pending
            logger.Verbose("Connecting to storage");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(backupContainer);
            logger.Verbose("Ensuring container");
            await blobContainer.CreateIfNotExistsAsync();
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });
            logger.Verbose("Ensuring blob");
            return blobContainer.GetBlockBlobReference($"{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}.trig");
        }

        private static async Task uploadBackup(CloudBlockBlob blob)
        {
            string graphDBBackupUrl = $"{dataAPI}/repositories/Master/statements?infer=false&context=null";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                client.DefaultRequestHeaders.Add("Api-Version", apiVersion);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-trig"));
                using (HttpResponseMessage response = await client.GetAsync(graphDBBackupUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (CloudBlobStream backupFileStream = blob.OpenWrite())
                    {
                        await response.Content.CopyToAsync(backupFileStream);
                    }
                }
            }
        }
    }
}