using Microsoft.ApplicationInsights;
using System;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace Parliament.Data.Orchestration.CoordinateTransformation.Controllers
{
    public class TransformationController : ApiController
    {
        public string Post([FromBody]string ring)
        {
            string polygon = convertEastingNorthingtoLongLat(ring);
            return polygon;
        }

        public string Get()
        {
            try
            {
                string workingDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/GridInQuestArtefacts");
                string tempPath = Path.GetTempPath();
                return $"{workingDirectory}|{tempPath}";
            }
            catch (Exception e)
            {
                return e.StackTrace;
            }
            
        }

        private string convertEastingNorthingtoLongLat(string ring)
        {
            string[] conversionOutput = null;
            string workingDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/GridInQuestArtefacts");
            string inputFile = $"{Path.GetTempPath()}{Guid.NewGuid().ToString()}.tmp";
            string outputFile = $"{Path.GetTempPath()}{Guid.NewGuid().ToString()}.tmp";
            string[] arguments =
                {
                    $"\"{workingDirectory}\\TransformationSettings.xml\"",
                    $"\"{inputFile}\"",
                    $"\"{outputFile}\""
                };
            File.WriteAllLines(inputFile, ring.Split(' '));
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = $"{workingDirectory}\\giqtrans.exe";
                process.StartInfo.Arguments = string.Join(" ", arguments);
                process.Start();
                while ((process.HasExited == false) || (File.Exists(outputFile) == false))
                { }
                process.Dispose();
                conversionOutput = File.ReadAllLines(outputFile);
            }
            catch (Exception e)
            {
                TelemetryClient telemetryClient = new TelemetryClient();
                telemetryClient.TrackException(e);
                return null;
            }
            finally
            {
                if (File.Exists(inputFile))
                    File.Delete(inputFile);
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
            }
            string[] longLat = conversionOutput.ToList()
                .Skip(1)
                .Select(line => string.Format("{0} {1}", line.Split(',')[3], line.Split(',')[2]))
                .ToArray();
            string polygon = string.Join(",", longLat);
            return $"({polygon})";
        }
    }
}
