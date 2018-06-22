using System;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.IdGenerator
{
    public class IdMaker
    {
        private static string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);
        protected Logger logger = new Logger(new Microsoft.Azure.WebJobs.ExecutionContext());

        public string MakeId()
        {
            if (idNamespace[idNamespace.Length - 1] != '/')
                idNamespace = $"{idNamespace}/";
            RandomStringGenerator generator = new RandomStringGenerator();
            string id = null;
            do
                id = $"{idNamespace}{generator.GetAlphanumericId(8)}";
            while (ExistsInTripleStore(id));
            return id;
        }

        private bool ExistsInTripleStore(string id)
        {
            string command = @"
                CONSTRUCT {?id a <https://example.org/object>.}
                WHERE {
                    BIND(@id AS ?id)
                    {?id ?p ?o.}
                    union
                    {?s ?p1 ?id.}
                } LIMIT 1";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.SetUri("id", new Uri(id));
            IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "false");
            if (graph.IsEmpty)
                return false;
            return true;
        }
    }
}