using System;

namespace Functions.IdGenerator
{
    public class IdMaker
    {
        private static string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);

        public string MakeId()
        {
            if (idNamespace[idNamespace.Length - 1] != '/')
                idNamespace = $"{idNamespace}/";
            RandomStringGenerator generator = new RandomStringGenerator();
            var id = generator.GetAlphanumericId(8);
            return $"{idNamespace}{id}";
        }
    }

}