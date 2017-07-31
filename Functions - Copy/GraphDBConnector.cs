using System;
using System.Collections.Generic;
using System.Net;
using VDS.RDF.Storage;

namespace Functions
{
    public class GraphDBConnector : SesameHttpProtocolVersion6Connector
    {
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);

        public GraphDBConnector(string queryString)
            : base(dataAPI, $"Master{queryString}")
        {
        }

        protected override HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
        {
            HttpWebRequest originalRequest = base.CreateRequest(servicePath, accept, method, queryParams);
            originalRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return originalRequest;
        }

    }
}
