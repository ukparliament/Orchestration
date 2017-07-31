using Newtonsoft.Json;
using Parliament.Ontology.Base;
using Parliament.Ontology.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Functions
{
    public class BaseTransformation<T> where T : ITransformationSettings, new()
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

        public virtual IBaseOntology[] TransformSource(string response)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, object> GetKeysForTarget(IBaseOntology[] deserializedSource)
        {
            return new Dictionary<string, object> { };
        }

        public virtual IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] deserializedTarget)
        {
            throw new NotImplementedException();
        }

        public virtual IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri subjectUri, T settings)
        {
            throw new NotImplementedException();
        }

        protected Uri GenerateNewId()
        {
            string id = new IdGenerator.IdMaker().MakeId();
            return new Uri(id);
        }

        private IGraph serializeSource(IBaseOntology[] source)
        {
            Graph result = null;
            try
            {
                logger.Verbose("Serializing source");
                Serializer serializer = new Serializer();
                result = serializer.Serialize(source, typeof(Parliament.Ontology.Code.House).Assembly, SerializerOptions.ExcludeRdfType);
                result.NamespaceMap.AddNamespace("parl", new Uri(schemaNamespace));
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            return result;
        }

        private async Task<HttpResponseMessage> communicateBack(string callbackUrl, string errorMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(errorMessage) == false)
                    logger.Error(errorMessage);
                logger.Finished();
                return await client.PostAsync(callbackUrl, new StringContent(errorMessage ?? "OK"));
            }
        }

        private Uri getSubject(Dictionary<string, object> subjectRetrievalDictionary, T settings)
        {
            Uri subjectUri = null;
            try
            {
                logger.Verbose("Getting key");
                SparqlParameterizedString sparql = new SparqlParameterizedString(settings.SubjectRetrievalSparqlCommand);
                sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                foreach (KeyValuePair<string, object> predicateValue in subjectRetrievalDictionary)
                    setSparqlParameter(sparql, predicateValue);
                subjectUri = IdRetrieval.GetSubject(sparql.ToString(), true, logger);
                logger.Verbose($"Subject: {subjectUri}");
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            return subjectUri;
        }

        private IGraph getExistingGraph(Uri subject, Dictionary<string, object> graphRetrievalDictionary, T settings)
        {
            SparqlParameterizedString sparql = new SparqlParameterizedString(settings.ExistingGraphSparqlCommand);
            sparql.SetUri("subject", subject);
            foreach (KeyValuePair<string, object> predicateValue in graphRetrievalDictionary)
                setSparqlParameter(sparql, predicateValue);
            sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            logger.Verbose("Trying to get existing graph");
            return GraphRetrieval.GetGraph(sparql.ToString(), logger);
        }

        private void setSparqlParameter(SparqlParameterizedString sparql, KeyValuePair<string, object> predicateValue)
        {
            object value = predicateValue.Value;
            if (value == null)
                sparql.SetLiteral(predicateValue.Key, string.Empty);
            else
                if (value is Uri)
                    sparql.SetUri(predicateValue.Key, (Uri)value);
                else
                    switch (Type.GetTypeCode(value.GetType().BaseType))
                    {
                        case TypeCode.Boolean:
                            sparql.SetLiteral(predicateValue.Key, (bool)value);
                            break;
                        case TypeCode.DateTime:
                            sparql.SetLiteral(predicateValue.Key, (DateTime)value);
                            break;
                        case TypeCode.Decimal:
                            sparql.SetLiteral(predicateValue.Key, (decimal)value);
                            break;
                        case TypeCode.Double:
                            sparql.SetLiteral(predicateValue.Key, (double)value);
                            break;
                        case TypeCode.Int32:
                            sparql.SetLiteral(predicateValue.Key, (int)value);
                            break;
                        case TypeCode.String:
                            sparql.SetLiteral(predicateValue.Key, value.ToString());
                            break;
                        default:
                            sparql.SetLiteral(predicateValue.Key, value.ToString());
                            break;
                    }
        }

        private IEnumerable<IBaseOntology> deserializeTarget(IGraph target)
        {
            IEnumerable<IBaseOntology> result = null;
            try
            {
                logger.Verbose("Deserialize target");
                Serializer serializer = new Serializer();
                result = serializer.Deserialize(target, typeof(Parliament.Ontology.Code.House).Assembly);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            return result;
        }

        private async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl, T settings)
        {
            string response = await DataRetrieval.DataFromUrl(settings.FullDataUrlParameterizedString(dataUrl), settings.AcceptHeader, logger);
            if (string.IsNullOrWhiteSpace(response))
                return await communicateBack(callbackUrl, "Problem while getting source data");

            IBaseOntology[] deserializedSource;
            try
            {
                logger.Verbose("Deserializing source");
                deserializedSource = TransformSource(response);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return await communicateBack(callbackUrl, "Problem while deserializing source");
            }

            Dictionary<string, object> subjectRetrievalDictionary;
            try
            {
                logger.Verbose("Retrieving key(s) from the source");
                subjectRetrievalDictionary = GetKeysFromSource(deserializedSource);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return await communicateBack(callbackUrl, "Problem while retrieving key(s) from the source");
            }

            Uri subjectUri = getSubject(subjectRetrievalDictionary, settings);
            if (subjectUri == null)
                return await communicateBack(callbackUrl, "Problem while obtaining subject from triplestore");

            Dictionary<string, object> graphRetrievalDictionary = GetKeysForTarget(deserializedSource);
            IGraph existingGraph = getExistingGraph(subjectUri, graphRetrievalDictionary, settings);
            if (existingGraph == null)
                return await communicateBack(callbackUrl, $"Problem while retrieving old graph for {subjectUri}");

            IBaseOntology[] deserializedTarget;
            deserializedTarget = deserializeTarget(existingGraph).ToArray();
            if (deserializedTarget == null)
                return await communicateBack(callbackUrl, $"Problem while deserializing target");

            IBaseOntology[] deserializedSourceWithIds;
            try
            {
                logger.Verbose("Assigning ids");
                deserializedSourceWithIds = SynchronizeIds(deserializedSource, subjectUri, deserializedTarget);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return await communicateBack(callbackUrl, "Problem while assigninig ids");
            }

            IGraph newGraph = serializeSource(deserializedSourceWithIds);
            if (newGraph == null)
                return await communicateBack(callbackUrl, $"Problem with retrieving new graph for {subjectUri}");

            IGraph existingGraphsWithoutTypes = new Graph();
            foreach (Triple t in existingGraph.Triples.Where(t => ((IUriNode)t.Predicate).Uri.ToString() != RdfSpecsHelper.RdfType))
                existingGraphsWithoutTypes.Assert(t);
            GraphDiffReport difference = existingGraphsWithoutTypes.Difference(newGraph);
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
