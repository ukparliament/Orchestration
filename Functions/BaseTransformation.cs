using Newtonsoft.Json;
using Parliament.Rdf.Serialization;
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

namespace Functions
{
    public class BaseTransformation<T,K> 
        where T : ITransformationSettings, new()        
    {
        protected Logger logger=new Logger(new Microsoft.Azure.WebJobs.ExecutionContext());

        protected static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);
        protected static readonly string idNamespace = Environment.GetEnvironmentVariable("IdNamespace", EnvironmentVariableTarget.Process);
        public static readonly string HouseOfCommonsId = "https://id.parliament.uk/1AFu55Hs";
        public static readonly string HouseOfLordsId = "https://id.parliament.uk/WkUWUBMx";

        public async Task<object> Run(HttpRequestMessage req, T settings, Microsoft.Azure.WebJobs.ExecutionContext executionContext)
        {
            logger = new Logger(executionContext);
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
            if (data.workflowId != null)
                logger.SetWorkflowId(data.workflowId.ToString());
            new Thread(() => startProcess(data.url.ToString(), data.callbackUrl.ToString(), settings)).Start();

            return req.CreateResponse();
        }

        public virtual K GetSource(string dataUrl, T settings)
        {
            throw new NotImplementedException();
        }

        public virtual BaseResource[] TransformSource(K response)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            return null;
        }

        public virtual Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return null;
        }

        public virtual Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            return new Dictionary<string, INode> { };
        }

        public virtual BaseResource[] SynchronizeIds(BaseResource[] source, Uri id, BaseResource[] deserializedTarget)
        {
            throw new NotImplementedException();
        }

        public virtual IGraph GenerateNewGraph(XDocument doc, IGraph oldGraph, Uri id, T settings)
        {
            throw new NotImplementedException();
        }

        public virtual IGraph AlterNewGraph(IGraph newGraph, Uri id, K response)
        {
            return newGraph;
        }

        protected Uri GenerateNewId()
        {
            string id = new IdGenerator.IdMaker().MakeId(logger);
            return new Uri(id);
        }

        private Type[] modelTypes
        {
            get
            {
                return typeof(Parliament.Model.House).Assembly.GetTypes();
            }
        }

        private IGraph serializeSource(BaseResource[] source)
        {
            Graph result = null;
            try
            {
                logger.Verbose("Serializing source");
                RdfSerializer serializer = new RdfSerializer();
                result = serializer.Serialize(source, modelTypes, SerializerOptions.ExcludeRdfType);
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

        private Uri getSubject(Dictionary<string, INode> subjectRetrievalDictionary, T settings)
        {
            Uri subjectUri = null;
            try
            {
                logger.Verbose("Getting key");
                SparqlConstructor sparqlConstructor = new SparqlConstructor(settings.SubjectRetrievalSparqlCommand, subjectRetrievalDictionary);
                sparqlConstructor.Sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
                subjectUri = IdRetrieval.GetSubject(sparqlConstructor.Sparql.ToString(), true, logger);
                logger.Verbose($"Subject: {subjectUri}");
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            return subjectUri;
        }

        private IGraph getExistingGraph(Uri subject, Dictionary<string, INode> graphRetrievalDictionary, T settings)
        {
            graphRetrievalDictionary = graphRetrievalDictionary ?? new Dictionary<string, INode>();
            graphRetrievalDictionary.Add("subject", SparqlConstructor.GetNode(subject));
            SparqlConstructor sparqlConstructor = new SparqlConstructor(settings.ExistingGraphSparqlCommand, graphRetrievalDictionary);
            sparqlConstructor.Sparql.Namespaces.AddNamespace("parl", new Uri(schemaNamespace));
            logger.Verbose("Trying to get existing graph");
            return GraphRetrieval.GetGraph(sparqlConstructor.Sparql.ToString(), logger);
        }

        private IEnumerable<BaseResource> deserializeTarget(IGraph target)
        {
            IEnumerable<BaseResource> result = null;
            try
            {
                logger.Verbose("Deserialize target");
                RdfSerializer serializer = new RdfSerializer();
                result = serializer.Deserialize(target, modelTypes, new Uri(schemaNamespace));
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
            return result;
        }

        private async Task<HttpResponseMessage> startProcess(string dataUrl, string callbackUrl, T settings)
        {
            K response = GetSource(dataUrl, settings);
            if (response==null)
                return await communicateBack(callbackUrl, "Problem while getting source data");

            BaseResource[] deserializedSource;
            try
            {
                logger.Verbose("Deserializing source");
                deserializedSource = TransformSource(response);
                if (deserializedSource == null)
                    return await communicateBack(callbackUrl, "Nothing was deserialized from the source");
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return await communicateBack(callbackUrl, "Problem while deserializing source");
            }

            Dictionary<string, INode> subjectRetrievalDictionary;
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

            Uri subjectUri = null;
            if (subjectRetrievalDictionary != null)
                subjectUri = getSubject(subjectRetrievalDictionary, settings);
            else
            {
                try
                {
                    subjectUri = GetSubjectFromSource(deserializedSource);
                }
                catch (Exception e)
                {
                    logger.Exception(e);
                    return await communicateBack(callbackUrl, "Problem while retrieving subject from the source");
                }
            }
            if (subjectUri == null)
                return await communicateBack(callbackUrl, "Problem while obtaining subject from triplestore");

            Dictionary<string, INode> graphRetrievalDictionary = GetKeysForTarget(deserializedSource);
            IGraph existingGraph = getExistingGraph(subjectUri, graphRetrievalDictionary, settings);
            if (existingGraph == null)
                return await communicateBack(callbackUrl, $"Problem while retrieving old graph for {subjectUri}");

            BaseResource[] deserializedTarget;
            deserializedTarget = deserializeTarget(existingGraph).ToArray();
            if (deserializedTarget == null)
                return await communicateBack(callbackUrl, $"Problem while deserializing target");

            BaseResource[] deserializedSourceWithIds;
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

            try
            {
                newGraph = AlterNewGraph(newGraph, subjectUri, response);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return await communicateBack(callbackUrl, "Problem while altering new graph");
            }

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
