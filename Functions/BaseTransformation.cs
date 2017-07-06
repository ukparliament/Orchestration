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
        protected Logger logger;

        protected static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);

        public async Task<object> Run(HttpRequestMessage req, T settings)
        {
            logger = new Logger();
            logger.SetOperationName(settings.OperationName);
            logger.Triggered();
            string jsonContent = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(jsonContent);

            if ((data.url == null) || (data.callbackUrl == null))
            {
                logger.Error("Missing some value(s)");
                logger.Finished();
                return req.CreateResponse(HttpStatusCode.BadRequest, "Missing some value(s)");
            }
            logger.SetDataUrl(data.url.ToString());
            if (data.batchId != null)
                logger.SetBatchId(data.batchId.ToString());
            new Thread(() => startProcess(data.url.ToString(), data.callbackUrl.ToString(), settings)).Start();

            return req.CreateResponse();
        }

        private async Task<HttpResponseMessage> communicateBack(string callbackUrl, string errorMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(errorMessage)==false)
                    logger.Error(errorMessage);
                logger.Finished();
                return await client.PostAsync(callbackUrl, new StringContent(errorMessage ?? "OK"));
            }
        }

        private Uri getSubject(XDocument doc, T settings)
        {
            Uri subjectUri = null;
            try
            {
                logger.Verbose("Getting key");
                SparqlParameterizedString sparql = new SparqlParameterizedString(settings.SubjectRetrievalSparqlCommand);
                sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                foreach (KeyValuePair<string, string> nameXPathPair in settings.SubjectRetrievalParameters)
                {
                    XElement key = doc.XPathSelectElement(nameXPathPair.Value, settings.SourceXmlNamespaceManager);
                    logger.Verbose($"Key {nameXPathPair.Key}: {key.Value}");
                    sparql.SetLiteral(nameXPathPair.Key, key.Value);
                }
                subjectUri = IdRetrieval.GetSubject(sparql.ToString(), true, logger);
                logger.Verbose($"Subject: {subjectUri}");
            }
            catch (Exception e)
            {
                logger.Exception(e);
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
            SparqlParameterizedString sparql = new SparqlParameterizedString(settings.ExistingGraphSparqlCommand);
            sparql.SetUri("subject", subject);
            if (settings.ExistingGraphSparqlParameters != null)
                foreach (KeyValuePair<string, string> nameXPathPair in settings.ExistingGraphSparqlParameters)
                {
                    XElement key = doc.XPathSelectElement(nameXPathPair.Value, settings.SourceXmlNamespaceManager);
                    sparql.SetLiteral(nameXPathPair.Key, key.Value);
                }
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            logger.Verbose("Trying to get existing graph");
            return GraphRetrieval.GetGraph(sparql.ToString(), logger);
        }

        private async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl, T settings)
        {
            XDocument doc = await XmlDataRetrieval.GetXmlDataFromUrl(settings.FullDataUrlParameterizedString(dataUrl), settings.AcceptHeader, logger);
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
            logger.Metric("AddedTriples", difference.AddedTriples.Count());
            logger.Metric("RemovedTriples", difference.RemovedTriples.Count());

            if ((difference.AddedTriples.Any() == false) && (difference.RemovedTriples.Any() == false))
            {
                logger.Verbose("No update required");
                return await communicateBack(callbackUrl);
            }
            bool isGraphUpdated = GraphUpdate.UpdateDifference(difference, logger);
            if (isGraphUpdated == false)
                return await communicateBack(callbackUrl, $"Problem when updating graph for {subjectUri}");
            return await communicateBack(callbackUrl);
        }

    }
}
