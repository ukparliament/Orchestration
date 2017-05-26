using System;

namespace Functions.IdGenerator
{
    public class IdMaker
    {
        private static string dataBaseUri = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);

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