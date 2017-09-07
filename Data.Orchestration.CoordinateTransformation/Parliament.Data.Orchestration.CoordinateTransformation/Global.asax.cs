using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Parliament.Data.Orchestration.CoordinateTransformation
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("TransformationController", "{controller}");
            GlobalConfiguration.Configuration.Formatters.Add(new TextPlainFormatter()); 
            GlobalConfiguration.Configuration.Services.Add(typeof(IExceptionLogger), new AIExceptionLogger());
        }

    }
}