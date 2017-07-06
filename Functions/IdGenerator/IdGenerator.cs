using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Functions.IdGenerator
{
    public static class IdGenerator
    {
        private static Logger logger;

        [FunctionName("IdGenerator")]
        public static async Task<string> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            logger.SetOperationName("IdGenerator");
            logger.Triggered();
            IdMaker generator = new IdMaker();
            string id = generator.MakeId();

            return id;
        }
    }
}