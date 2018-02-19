using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;

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

    }
}
