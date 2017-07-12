using System;
using System.Diagnostics;
using System.Linq;
using VDS.RDF;

namespace Functions
{
    public static class GraphUpdate
    {
        public static bool UpdateDifference(GraphDiffReport difference, Logger logger)
        {
            Stopwatch externalTimer = Stopwatch.StartNew();
            DateTime externalStartTime = DateTime.UtcNow;
            bool externalCallOk = true;
            try
            {
                logger.Verbose("Updating graph");
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
                logger.Exception(e);
            }
            finally
            {
                externalTimer.Stop();
                logger.Dependency("GraphUpdate", $"+{difference.AddedTriples.Count()}/-{difference.RemovedTriples.Count()}", externalStartTime, externalTimer.Elapsed, externalCallOk);
            }
            return externalCallOk;
        }
    }
}
