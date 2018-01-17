using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace Functions
{
    public class GraphDBConnector : SesameHttpProtocolVersion6Connector
    {
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);

        public GraphDBConnector(string queryString)
            : base(dataAPI, $"Master?{queryString}")
        {
        }

        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            StringBuilder sb = new StringBuilder();
            SparqlFormatter sparqlFormatter = new SparqlFormatter();
            if ((removals != null) && (removals.Any()))
            {
                sb.AppendLine("DELETE DATA {");
                foreach (Triple removed in removals)
                    sb.AppendLine(sparqlFormatter.Format(removed));
                sb.Append("}");
                if ((additions != null) && (additions.Any()))
                    sb.AppendLine(";");
                else
                    sb.AppendLine();
            }
            if ((additions != null) && (additions.Any()))
            {
                sb.AppendLine("INSERT DATA {");
                foreach (Triple added in additions)
                    sb.AppendLine(sparqlFormatter.Format(added));
                sb.AppendLine("}");                
            }
            string sparqlUpdate = sb.ToString();
            SparqlUpdateParser parser = new SparqlUpdateParser();
            parser.ParseFromString(sparqlUpdate);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{BaseUri}{this._repositoriesPrefix}Master{this._updatePath}");
            request.Method = "POST";
            request.ContentType = "application/sparql-update";
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(Options.UseBomForUtf8)))
            {
                writer.Write(sparqlUpdate);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Dispose();
        }

        protected override HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
        {
            HttpWebRequest originalRequest = base.CreateRequest(servicePath, accept, method, queryParams);
            originalRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return originalRequest;
        }

    }

}
