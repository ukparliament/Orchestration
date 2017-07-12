using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions
{
    public static class IdRetrieval
    {
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);

        public static Uri GetSubject(string subjectType, string predicate, string objectValue, bool canCreateNewId, Logger logger)
        {
            string command = @"
                construct{
                    ?s a @subjectType.
                }
                where{
                    ?s a @subjectType; 
                        @predicate @objectValue.
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.SetUri("subjectType", new Uri($"{schemaNamespace}{subjectType}"));
            sparql.SetUri("predicate", new Uri($"{schemaNamespace}{predicate}"));
            sparql.SetLiteral("objectValue", objectValue);

            return getValue(sparql.ToString(), canCreateNewId, logger);
        }
        public static Uri GetSubject(string sparql, bool canCreateNewId, Logger logger)
        {
            return getValue(sparql, canCreateNewId, logger);
        }
        public static Dictionary<string, string> GetSubjects(string subjectType, string predicate, Logger logger)
        {
           string command = @"
                construct{
                    ?s @predicate ?objectValue.
                }
                where{
                    ?s a @subjectType; 
                        @predicate ?objectValue.
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.SetUri("subjectType", new Uri($"{schemaNamespace}{subjectType}"));
            sparql.SetUri("predicate", new Uri($"{schemaNamespace}{predicate}"));
            Dictionary<string, string> result = new Dictionary<string, string>();
            IGraph graph = GraphRetrieval.GetGraph(sparql.ToString(), logger, "false");
            foreach (Triple triple in graph.Triples)
            {
                result.Add(triple.Object.ToString(), triple.Subject.ToString());
            }
            return result;
        }

        private static Uri getValue(string sparql, bool canCreateNewId, Logger logger)
        {
            IGraph graph = GraphRetrieval.GetGraph(sparql, logger, "false");
            if (graph.IsEmpty)
            {
                if (canCreateNewId == true)
                {
                    string id = new IdGenerator.IdMaker().MakeId();
                    logger.Verbose($"Created new id ({id})");
                    return new Uri(id);
                }
                else
                {
                    logger.Verbose("Not found");
                    return null;
                }
            }
            else
            {
                Uri result = ((IUriNode)graph.Triples.SubjectNodes.SingleOrDefault()).Uri;
                logger.Verbose($"Found existing ({result})");
                return result;
            }
        }

    }
}
