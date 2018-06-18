using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Functions.Test
{
    [Ignore]
    [TestClass]
    public class TransformationTest
    {
        private readonly Uri baseUri = new Uri("https://id.parliament.uk");
        private readonly Uri schemaNamespace = new Uri("https://id.parliament.uk/schema/");

        public TransformationTest()
        {
            Environment.SetEnvironmentVariable("idNamespace", baseUri.ToString());
            Environment.SetEnvironmentVariable("schemaNamespace", schemaNamespace.ToString());
            Environment.SetEnvironmentVariable("CUSTOMCONNSTR_Data", "https://api.parliament.uk/rdf4j");
            Environment.SetEnvironmentVariable("SubscriptionKey", "");
            Environment.SetEnvironmentVariable("ApiVersion", "");
            Environment.SetEnvironmentVariable("ApplicationInsightsInstrumentationKey", "abc");
        }

        private static IEnumerable<object[]> transformationNames
        {
            get
            {
                return typeof(ITransformationSettings).Assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i == typeof(ITransformationSettings)))
                    .Select(t => new[] { t.Namespace.Split('.').Last() });                    
            }
        }

        [TestMethod]
        public void SourceDataNumberTest()
        {
            Assert.AreEqual(transformationNames.Count(), Assembly.GetExecutingAssembly().GetManifestResourceNames().Count());
        }

        [TestMethod]
        [DynamicData(nameof(transformationNames))]
        public void SimpleTransformationTest(string transformationName)
        {
            if (transformationName == "TransformationEPetition")
                return;
            ITransformationSettings settings = Activator.CreateInstance(typeof(ITransformationSettings).Assembly.GetType($"Functions.{transformationName}.Settings")) as ITransformationSettings;
            object transformation = Activator.CreateInstance(typeof(ITransformationSettings).Assembly.GetType($"Functions.{transformationName}.Transformation"));

            string sourceData = getResourceFile($"{transformationName}.txt");
            BaseResource[] source = transformation.GetType()
                .GetMethod("TransformSource")
                .Invoke(transformation, new object[] { sourceData }) as BaseResource[];
            Uri id = transformation.GetType()
                .GetMethod("GetSubjectFromSource")
                .Invoke(transformation, new object[] { source }) as Uri;
            if (id==null)
                id = generateNewId();
            source = transformation.GetType()
                .GetMethod("SynchronizeIds")
                .Invoke(transformation, new object[] { source, id, new BaseResource[] { } }) as BaseResource[];
            Graph sourceGraph = serialize(source);

            Dictionary<string, INode> graphRetrievalDictionary = transformation.GetType()
                .GetMethod("GetKeysForTarget")
                .Invoke(transformation, new object[] { source }) as Dictionary<string, INode>;
            graphRetrievalDictionary = graphRetrievalDictionary ?? new Dictionary<string, INode>();            
            graphRetrievalDictionary.Add("subject", SparqlConstructor.GetNode(id));
            SparqlConstructor sparqlConstructor = new SparqlConstructor(settings.ExistingGraphSparqlCommand, graphRetrievalDictionary);
            sparqlConstructor.Sparql.Namespaces.AddNamespace("parl", schemaNamespace);

            Graph targetGraph = sourceGraph.ExecuteQuery(sparqlConstructor.Sparql.ToString()) as Graph;
            IEnumerable<BaseResource> target = deserialize(targetGraph);
            BaseResource[] synchronizedSource = transformation.GetType()
                .GetMethod("SynchronizeIds")
                .Invoke(transformation, new object[] { source, id, target }) as BaseResource[];
            Graph sourceSerialized = serialize(synchronizedSource);
            Graph targetGraphWithoutTypes = new Graph();
            foreach (Triple t in targetGraph.Triples.Where(t => ((IUriNode)t.Predicate).Uri.ToString() != RdfSpecsHelper.RdfType))
                targetGraphWithoutTypes.Assert(t);
            GraphDiffReport difference = sourceSerialized.Difference(targetGraphWithoutTypes);

            Assert.IsTrue(sourceSerialized.Triples.Any());
            Assert.IsTrue(difference.AreEqual);
        }

        private Graph serialize(BaseResource[] source)
        {
            RdfSerializer serializer = new RdfSerializer();
            return serializer.Serialize(source, typeof(House).Assembly.GetTypes(), SerializerOptions.ExcludeRdfType);
        }

        private IEnumerable<BaseResource> deserialize(Graph target)
        {
            RdfSerializer serializer = new RdfSerializer();
            return serializer.Deserialize(target, typeof(House).Assembly.GetTypes(), baseUri);
        }

        private Uri generateNewId()
        {
            string id = new IdGenerator.IdMaker().MakeId();
            return new Uri(id);
        }

        private string getResourceFile(string resourceName)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Functions.Test.SourceData.{resourceName}"))
            using (StreamReader reader = new StreamReader(resourceStream))
                return reader.ReadToEnd();
        }
    }
}
