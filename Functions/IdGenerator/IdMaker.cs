using System;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.IdGenerator
{
    public class IdMaker
    {
        private static string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);
        
        public string MakeId(Logger logger)
        {
            if (idNamespace[idNamespace.Length - 1] != '/')
                idNamespace = $"{idNamespace}/";
            RandomStringGenerator generator = new RandomStringGenerator();
            string id = null;
            do
                id = $"{idNamespace}{generator.GetAlphanumericId(8)}";
            while (ExistsInTripleStore(id, logger));
            return id;
        }

        private bool ExistsInTripleStore(string id, Logger logger)
        {
            string command = @"ask{
                {@id ?p ?o.}
                union
                {?s ?p1 @id.}
            }";                    
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.SetUri("id", new Uri(id));
            bool? result = GraphRetrieval.GetAskQueryResult(sparql.ToString(), logger, "true");
            if (result.HasValue == false)
                throw new ArgumentOutOfRangeException("No response from ask query", new Exception("Possible connectivity issue"));
            return true;
        }
    }
}