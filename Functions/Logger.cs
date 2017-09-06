﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Functions
{
    public class Logger
    {
        private readonly SeverityLevel lowestLoggingLevel;
        public Logger()
        {
            string lowestLoggingLevelString = Environment.GetEnvironmentVariable("LowestLoggingLevel", EnvironmentVariableTarget.Process);
            if (Enum.TryParse(lowestLoggingLevelString, true, out lowestLoggingLevel) == false)
                lowestLoggingLevel = SeverityLevel.Information;
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
        public void Verbose(string message)
        {
            TrackTrace(message, SeverityLevel.Verbose);
        }
        public void Warning(string message)
        {
            TrackTrace(message, SeverityLevel.Warning);
        }

        public void Error(string message, Dictionary<string, string> properties = null)
        {
            TrackTrace(message, SeverityLevel.Error);
        }

        private void TrackTrace(string message, SeverityLevel severityLevel, Dictionary<string, string> properties = null)
        {
            if ((int)severityLevel >= (int)lowestLoggingLevel)
                telemetryClient.TrackTrace(message, severityLevel, properties);
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