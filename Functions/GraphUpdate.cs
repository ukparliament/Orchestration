using Microsoft.ApplicationInsights;
using System;
using System.Diagnostics;
using System.Linq;
using VDS.RDF;

namespace Functions
{
    public static class GraphUpdate
    {
        public static bool UpdateDifference(GraphDiffReport difference, TelemetryClient telemetryClient)
        {
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;
            try
            {
                telemetryClient.TrackTrace("Updating graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                using (GraphDBConnector connector = new GraphDBConnector(string.Empty))
                {
                    connector.Timeout = 180 * 1000;
                    Uri uri = null;
                    connector.UpdateGraph(uri, difference.AddedTriples, difference.RemovedTriples);
                }
            }
            catch (Exception e)
            {
                externalCallOk = false;
                telemetryClient.TrackException(e);
            }
            finally
            {
                externalTimer.Stop();
                telemetryClient.TrackDependency("GraphUpdate", $"+{difference.AddedTriples.Count()}/-{difference.RemovedTriples.Count()}", externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return externalCallOk;
        }
    }
}
