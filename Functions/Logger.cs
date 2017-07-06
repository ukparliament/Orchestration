using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions
{
    public class Logger
    {
        public Logger()
        {
            setOperationId();
        }
        private TelemetryClient telemetryClient = new TelemetryClient()
        {
            InstrumentationKey = Environment.GetEnvironmentVariable("ApplicationInsightsInstrumentationKey", EnvironmentVariableTarget.Process)
        };

        private Stopwatch timer;
        public void Triggered()
        {
            timer = Stopwatch.StartNew();
            telemetryClient.TrackEvent("Triggered");
        }
        public void Finished()
        {
            timer.Stop();
            telemetryClient.TrackEvent("Finished", metrics: new Dictionary<string, double>() { { "ProcessTime", timer.Elapsed.TotalMilliseconds } });
        }
        public void MessageCount(int OriginalMessageCount, int RetrievedMessageCount)
        {
            telemetryClient.TrackEvent("Message count", new Dictionary<string, string>(), new Dictionary<string, double>() { { "OriginalMessageCount", OriginalMessageCount }, { "RetrievedMessageCount", RetrievedMessageCount } });

        }
        public void Error(string message)
        {
            telemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error);
        }
        public void Error(string message, Dictionary<string, string> properties)
        {
            telemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error, properties);
        }
        public void Verbose(string message)
        {
            telemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);
        }
        public void Warning(string message)
        {
            telemetryClient.TrackTrace(message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);
        }

        private void setOperationId()
        {
            telemetryClient.Context.Operation.Id = Guid.NewGuid().ToString();
        }

        public void SetOperationName(string operationName)
        {
            telemetryClient.Context.Operation.Name = operationName;
        }
                                
        public void SetDataUrl(string url)
        {
            telemetryClient.Context.Properties["DataUrl"] = url;
        }
        public void SetBatchId(string batchId)
        {
            telemetryClient.Context.Properties["BatchId"] = batchId;
        }
        public void SetQueueName(string queueName)
        {
            telemetryClient.Context.Properties["QueueName"] = queueName;
        }

        public void Exception(Exception e)
        {
            telemetryClient.TrackException(e);
        }

        public void Metric(string metric, int count)
        {
            telemetryClient.TrackMetric(metric, count);
        }

        public void Dependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool isSuccessful)
        {
            telemetryClient.TrackDependency(dependencyName, commandName, startTime, duration, isSuccessful);
        }

    }
}
