using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Functions.IdGenerator
{
    public class IdMaker
    {
        private static string dataBaseUri = Environment.GetEnvironmentVariable("DataBaseUri", EnvironmentVariableTarget.Process);

        public string MakeId()
        {
            if (dataBaseUri[dataBaseUri.Length - 1] != '/')
                dataBaseUri = $"{dataBaseUri }/";
            RandomStringGenerator generator = new RandomStringGenerator();
            var id = generator.GetAlphanumericId(8);
            return $"{dataBaseUri}{id}";
        }
    }

}