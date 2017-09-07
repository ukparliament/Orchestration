using Microsoft.ApplicationInsights;
using System.Web.Http.ExceptionHandling;

namespace Parliament.Data.Orchestration.CoordinateTransformation
{
    public class AIExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {
                var ai = new TelemetryClient();
                ai.TrackException(context.Exception);
            }

            base.Log(context);
        }
    }
}