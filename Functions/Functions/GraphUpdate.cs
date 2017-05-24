using System;
using VDS.RDF;

namespace Functions
{
    public static class GraphUpdate
    {
        public static void UpdateDifference(GraphDiffReport difference)
        {
            using (GraphDBConnector connector = new GraphDBConnector(string.Empty))
            {
                connector.Timeout = 180 * 1000;
                Uri uri = null;
                connector.UpdateGraph(uri, difference.AddedTriples, difference.RemovedTriples);
            }
        }
    }
}
