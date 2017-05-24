using Microsoft.ApplicationInsights;
using System;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;

namespace Functions
{
    public static class GraphRetrieval
    {
        public static IGraph GetGraph(string constructQuery, TelemetryClient telemetryClient, string infer = "false")
        {
            IGraph graph = makeCall(constructQuery, telemetryClient, infer);
            if (graph == null)
            {
                telemetryClient.TrackTrace("Second attempt.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                graph = makeCall(constructQuery, telemetryClient, infer);
            }
            return graph;
        }

        private static IGraph makeCall(string constructQuery, TelemetryClient telemetryClient, string infer = "false")
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
                telemetryClient.TrackException(e);
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("ExecuteGraphQuery", constructQuery, externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return graph;
        }

        private static IGraph executeGraphQuery(string sparql, string infer)
        {
            IGraph result = new Graph();
            using (GraphDBConnector connector = new GraphDBConnector($"?infer={infer}"))
            {
                connector.Timeout = 180 * 1000;
                GraphHandler rdfHandler = new GraphHandler(result);
                connector.Query(rdfHandler, null, sparql);
            }
            return result;
        }

    }
}
