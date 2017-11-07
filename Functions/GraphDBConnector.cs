using System;
using VDS.RDF.Storage;

namespace Functions
{
    public class GraphDBConnector : ReadWriteSparqlConnector
    {
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);
        private static readonly string dataEndpoint = $"{dataAPI}/repositories/Master";            

        public GraphDBConnector(string queryString)
            : base(new Uri($"{dataEndpoint}?Subscription-Key={subscriptionKey}&{queryString}"), new Uri($"{dataEndpoint}/statements?Subscription-Key={subscriptionKey}"))
        {
        }
    }
}
