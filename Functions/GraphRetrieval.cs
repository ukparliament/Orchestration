using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace Functions
{
    public static class GraphRetrieval
    {
        public static List<Uri> GetSubjects(string constructQuery, Logger logger, string infer = "false")
        {
            IGraph graph = makeCall(constructQuery, logger, infer);
            if (graph.IsEmpty)
                return new List<Uri>();
            if (graph == null)
                return null;
            return graph.Triples.SubjectNodes.Select(s => ((IUriNode)s).Uri).ToList();
        }

        public static IGraph GetGraph(string constructQuery, Logger logger, string infer = "false")
        {
            IGraph graph = makeCall(constructQuery, logger, infer);
            if (graph == null)
            {
                logger.Verbose("Second attempt.");
                graph = makeCall(constructQuery, logger, infer);
            }
            return graph;
        }

        public static bool? GetAskQueryResult(string askQuery, Logger logger, string infer = "false")
        {
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            SparqlResultSet result = null;
            bool externalCallOk = true;

            try
            {
                result = executeSparqlResultQuery(askQuery, infer);
            }
            catch (Exception e)
            {
                externalCallOk = false;
                logger.Exception(e);
            }
            finally
            {
                externalTimer.Stop();
                logger.Dependency("ExecuteAskQuery", askQuery, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return result?.Result;
        }

        private static IGraph makeCall(string constructQuery, Logger logger, string infer = "false")
        {
            IGraph graph = null;
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;
            try
            {
                graph = executeGraphQuery(constructQuery, infer);
            }
            catch (VDS.RDF.Parsing.RdfParserSelectionException e)
            {
                logger.Verbose(e.Message);
                graph = new Graph();
            }
            catch (Exception e)
            {
                externalCallOk = false;
                logger.Exception(e);
            }
            finally
            {
                externalTimer.Stop();
                logger.Dependency("ExecuteGraphQuery", constructQuery, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return graph;
        }

        private static IGraph executeGraphQuery(string sparql, string infer)
        {
            IGraph result = new Graph();
            using (GraphDBConnector connector = new GraphDBConnector($"infer={infer}"))
            {
                connector.Timeout = 180 * 1000;
                GraphHandler rdfHandler = new GraphHandler(result);
                connector.Query(rdfHandler, null, sparql);
            }
            return result;
        }

        private static SparqlResultSet executeSparqlResultQuery(string sparql, string infer)
        {
            SparqlResultSet result = new SparqlResultSet();
            using (GraphDBConnector connector = new GraphDBConnector($"infer={infer}"))
            {
                connector.Timeout = 180 * 1000;
                ResultSetHandler sparqlHandler = new ResultSetHandler(result);
                connector.Query(null, sparqlHandler, sparql);
            }
            return result;
        }

    }
}
