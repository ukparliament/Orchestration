using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions
{
    public class BaseTransformation<T> where T: ITransformationSettings, new()
    {
        protected readonly TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };
        protected static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        private readonly Stopwatch timer = Stopwatch.StartNew();

        public async Task<object> Run(HttpRequestMessage req, T settings)
        {
            telemetryClient.Context.Operation.Name = settings.OperationName;
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();
            
            telemetryClient.TrackEvent("Triggered");

            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if ((data.url == null) || (data.callbackUrl == null))
            {
                timer.Stop();
                telemetryClient.TrackTrace("Missing some value(s)", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                telemetryClient.TrackEvent("Finished", metrics: new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            telemetryClient.Context.Properties["DataUrl"] = data.url.ToString();
            if (data.batchId != null)
                telemetryClient.Context.Properties["BatchId"] = data.batchId.ToString();
            new Thread(() => startProcess(data.url.ToString(), data.callbackUrl.ToString(), settings)).Start();

            return req.CreateResponse();
        }

        private async Task<HttpResponseMessage> communicateBack(string callbackUrl, string errorMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                timer.Stop();
                if (string.IsNullOrWhiteSpace(errorMessage)==false)
                    telemetryClient.TrackTrace(errorMessage, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
                telemetryClient.TrackEvent("Finished", metrics: new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
                return await client.PostAsync(callbackUrl, new StringContent(errorMessage ?? "OK"));
            }
        }

        private Uri getSubject(XDocument doc, T settings)
        {
            Uri subjectUri = null;
            try
            {
                telemetryClient.TrackTrace("Getting key", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                SparqlParameterizedString sparql = new SparqlParameterizedString(settings.SubjectRetrievalSparqlCommand);
                sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                foreach (KeyValuePair<string, string> nameXPathPair in settings.SubjectRetrievalParameters)
                {
                    XElement key = doc.XPathSelectElement(nameXPathPair.Value, settings.SourceXmlNamespaceManager);
                    telemetryClient.TrackTrace($"Key {nameXPathPair.Key}: {key.Value}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    sparql.SetLiteral(nameXPathPair.Key, key.Value);
                }
                subjectUri = IdRetrieval.GetSubject(sparql.ToString(), true, telemetryClient);
                telemetryClient.TrackTrace($"Subject: {subjectUri}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            }
            catch (Exception e)
            {
                telemetryClient.TrackException(e);
                return null;
            }
            return subjectUri;
        }

        public virtual IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, T settings)
        {
            throw new NotImplementedException();
        }

        private IGraph getExistingGraph(Uri subject, XDocument doc, T settings)
        {
            SparqlParameterizedString sparql = new SparqlParameterizedString(settings.ExisitngGraphSparqlCommand);
            sparql.SetUri("subject", subject);
            if (settings.ExistingGraphSparqlParameters != null)
                foreach (KeyValuePair<string, string> nameXPathPair in settings.ExistingGraphSparqlParameters)
                {
                    XElement key = doc.XPathSelectElement(nameXPathPair.Value, settings.SourceXmlNamespaceManager);
                    sparql.SetLiteral(nameXPathPair.Key, key.Value);
                }
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));

            telemetryClient.TrackTrace("Trying to get existing graph", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
            return GraphRetrieval.GetGraph(sparql.ToString(), telemetryClient);
        }

        private async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl, T settings)
        {
            XDocument doc = await XmlDataRetrieval.GetXmlDataFromUrl(settings.FullDataUrlParameterizedString(dataUrl), settings.AcceptHeader, telemetryClient);
            if (doc == null)
                return await communicateBack(callbackUrl, "Problem while getting source data");

            Uri subjectUri = getSubject(doc, settings);
            if (subjectUri == null)
                return await communicateBack(callbackUrl, "Problem while obtaining subject from triplestore");

            IGraph existingGraph = getExistingGraph(subjectUri, doc, settings);
            if (existingGraph == null)
                return await communicateBack(callbackUrl, $"Problem while retrieving old graph for {subjectUri}");

            IGraph newGraph = GenerateNewGraph(doc, existingGraph, subjectUri, settings);
            if (newGraph == null)
                return await communicateBack(callbackUrl, $"Problem with retrieving new graph for {subjectUri}");

            GraphDiffReport difference = existingGraph.Difference(newGraph);
            telemetryClient.TrackMetric("AddedTriples", difference.AddedTriples.Count());
            telemetryClient.TrackMetric("RemovedTriples", difference.RemovedTriples.Count());

            if ((difference.AddedTriples.Any() == false) && (difference.RemovedTriples.Any() == false))
            {
                telemetryClient.TrackTrace("No update required", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                return await communicateBack(callbackUrl);
            }
            bool isGraphUpdated = GraphUpdate.UpdateDifference(difference, telemetryClient);
            if (isGraphUpdated == false)
                return await communicateBack(callbackUrl, $"Problem when updating graph for {subjectUri}");
            return await communicateBack(callbackUrl);
        }

    }
}
