using Microsoft.ApplicationInsights;
using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions
{
    public static class IdRetrieval
    {
        private static readonly string schemaNamespace = Environment.GetEnvironmentVariable("SchemaNamespace", EnvironmentVariableTarget.Process);

        public static Uri GetSubject(string subjectType, string predicate, string objectValue, bool canCreateNewId, TelemetryClient telemetryClient)
        {
            string command = @"
                construct{
                    ?s a @subjectType.
                }
                where{
                    ?s a @subjectType; 
                        @predicate @objectValue.
                }";
            SparqlParameterizedString sparql = new SparqlParameterizedString(command);
            sparql.SetUri("subjectType", new Uri($"{schemaNamespace}{subjectType}"));
            sparql.SetUri("predicate", new Uri($"{schemaNamespace}{predicate}"));
            sparql.SetLiteral("objectValue", objectValue);

            return getValue(sparql.ToString(), canCreateNewId, telemetryClient);
        }

        public static Uri GetSubject(string sparql, bool canCreateNewId, TelemetryClient telemetryClient)
        {
            return getValue(sparql, canCreateNewId, telemetryClient);
        }

        private static Uri getValue(string sparql, bool canCreateNewId, TelemetryClient telemetryClient)
        {
            IGraph graph = GraphRetrieval.GetGraph(sparql, telemetryClient, "false");
            if (graph.IsEmpty)
            {
                if (canCreateNewId == true)
                {
                    string id = new IdGenerator.IdMaker().MakeId();
                    telemetryClient.TrackTrace($"Created new id ({id})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    return new Uri(id);
                }
                else
                {
                    telemetryClient.TrackTrace("Not found", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                    return null;
                }
            }
            else
            {
                Uri result = ((IUriNode)graph.Triples.SubjectNodes.SingleOrDefault()).Uri;
                telemetryClient.TrackTrace($"Found existing ({result})", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
                return result;
            }
        }

    }
}
