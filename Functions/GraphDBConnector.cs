using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Parsing;
using System.IO;

namespace Functions
{
    public class GraphDBConnector : ReadWriteSparqlConnector
    {
        private static readonly string dataAPI = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Data", EnvironmentVariableTarget.Process);        
        private static readonly string dataEndpoint = $"{dataAPI}/repositories/Master";
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);

        public GraphDBConnector(string queryString)
            : base(new GraphDBRemoteEndpoint(new Uri($"{dataEndpoint}?{queryString}")), new GraphDBUpdateEndpoint(new Uri($"{dataEndpoint}/statements")))
        {
        }

        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            StringBuilder sb = new StringBuilder();
            SparqlFormatter sparqlFormatter = new SparqlFormatter();
            if ((additions != null) && (additions.Any()))
            {
                sb.AppendLine("INSERT DATA {");
                foreach (Triple added in additions)
                    sb.AppendLine(sparqlFormatter.Format(added));
                sb.Append("}");
                if ((removals != null) && (removals.Any()))
                    sb.AppendLine(";");
                else
                    sb.AppendLine();
            }
            if ((removals != null) && (removals.Any()))
            {
                sb.AppendLine("DELETE DATA {");
                foreach (Triple removed in removals)
                    sb.AppendLine(sparqlFormatter.Format(removed));
                sb.AppendLine("}");
            }
            string sparqlUpdate = sb.ToString();
            SparqlUpdateParser parser = new SparqlUpdateParser();
            parser.ParseFromString(sparqlUpdate);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UpdateEndpoint.Uri);
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

    }

    public class GraphDBRemoteEndpoint : SparqlRemoteEndpoint
    {
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);

        public GraphDBRemoteEndpoint(Uri endpointUri)
                : base(endpointUri)
        {            
            Proxy = new WebProxy(); //workaround for https://github.com/dotnetrdf/dotnetrdf/issues/103
        }

        protected override void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {            
            httpRequest.Proxy = null; //workaround for https://github.com/dotnetrdf/dotnetrdf/issues/103
            httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        }
    }

    public class GraphDBUpdateEndpoint : SparqlRemoteUpdateEndpoint
    {
        private static readonly string subscriptionKey = Environment.GetEnvironmentVariable("SubscriptionKey", EnvironmentVariableTarget.Process);

        public GraphDBUpdateEndpoint(Uri endpointUri)
                : base(endpointUri)
        {
            Proxy = new WebProxy(); //workaround for https://github.com/dotnetrdf/dotnetrdf/issues/103
        }
        
        protected override void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {
            httpRequest.Proxy = null; //workaround for https://github.com/dotnetrdf/dotnetrdf/issues/103
            httpRequest.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);            
        }        
    }
}
