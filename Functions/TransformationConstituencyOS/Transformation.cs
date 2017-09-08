using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Functions.TransformationConstituencyOS
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
        {
            RDF sourceConstituency = new RDF();
            using (StringReader reader = new StringReader(response))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RDF));
                sourceConstituency = (RDF)serializer.Deserialize(reader);
            }
            IOnsConstituencyGroup constituency = new OnsConstituencyGroup();
            if ((sourceConstituency != null) && (sourceConstituency.Description != null))
            {
                constituency.ConstituencyGroupOnsCode = sourceConstituency.Description.gssCode;
                if (sourceConstituency.Description.asGML != null)
                    constituency.ConstituencyGroupHasConstituencyArea = generateConstituencyArea(sourceConstituency.Description);
            }
            return new IBaseOntology[] { constituency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string constituencyGroupOnsCode = deserializedSource.OfType<IOnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode;
            return new Dictionary<string, object>()
            {
                { "constituencyGroupOnsCode", constituencyGroupOnsCode }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IOnsConstituencyGroup constituency = source.OfType<IOnsConstituencyGroup>().SingleOrDefault();
            constituency.SubjectUri = subjectUri;
            IConstituencyArea constituencyArea = target.OfType<IConstituencyArea>().SingleOrDefault();
            if ((constituencyArea != null) && (constituency.ConstituencyGroupHasConstituencyArea != null))
                constituency.ConstituencyGroupHasConstituencyArea.SubjectUri = constituencyArea.SubjectUri;

            return new IBaseOntology[] { constituency };
        }

        private IConstituencyArea generateConstituencyArea(RDFDescription description)
        {
            IConstituencyArea constituencyArea = new ConstituencyArea();
            constituencyArea.SubjectUri = GenerateNewId();
            if (description.lat != null)
                constituencyArea.ConstituencyAreaLatitude = description.lat.Value;
            if (description.@long != null)
                constituencyArea.ConstituencyAreaLongitude = description.@long.Value;
            if ((description.asGML != null) && (string.IsNullOrWhiteSpace(description.asGML.Value) == false))
            {
                string xmlPolygon = description.asGML.Value.ToString().Replace("gml:", string.Empty);
                XElement[] polygons = XDocument.Parse(xmlPolygon)
                    .Descendants("coordinates")
                    .ToArray();
                List<string> areas = generatePolygonArea(polygons);
                constituencyArea.ConstituencyAreaExtent = areas;
            }
            return constituencyArea;
        }

        private List<string> generatePolygonArea(XElement[] polygons)
        {
            int i = 0;
            List<string> areas = new List<string>();
            while (i < polygons.Length)
            {
                if (polygons[i].Parent.Parent.Name.LocalName == "outerBoundaryIs")
                {
                    string polygon = generatePolygon(polygons[i].Value);
                    if (polygon == null)
                        return new List<string>();
                    if ((i < polygons.Length - 1) && (polygons[i + 1].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                    {
                        i++;
                        while ((i < polygons.Length) && (polygons[i].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                        {
                            string innerPolygon = generatePolygon(polygons[i].Value);
                            if (innerPolygon == null)
                                return new List<string>();
                            polygon = $"{polygon},{innerPolygon}";
                            i++;
                        }
                    }
                    areas.Add($"Polygon({polygon})");
                }
                i++;
            }

            return areas;
        }

        private string generatePolygon(string ring)
        {
            string[] conversionOutput = null;
            string gridInQuestPath = Path.Combine(functionExecutionContext.FunctionDirectory, "GridInQuestArtefacts");
            string tempPath = Path.Combine(functionExecutionContext.FunctionDirectory, "temp");
            string fileName = Guid.NewGuid().ToString();
            string inputFile = $"{tempPath}\\{fileName}.in";
            string outputFile = $"{tempPath}\\{fileName}.out";
            string[] arguments =
                {
                    $"\"{gridInQuestPath}\\TransformationSettings.xml\"",
                    $"\"{inputFile}\"",
                    $"\"{outputFile}\""
                };
            if (Directory.Exists(tempPath) == false)
                Directory.CreateDirectory(tempPath);
            File.WriteAllLines(inputFile, ring.Split(' '));
            try
            {
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = $"{gridInQuestPath}\\giqtrans.exe";
                    process.StartInfo.Arguments = string.Join(" ", arguments);
                    process.Start();
                    process.WaitForExit();
                }
                conversionOutput = File.ReadAllLines(outputFile);
            }
            catch (Exception e)
            {
                logger.Exception(e);
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

        private string convertEastingNorthingtoLongLat(string eastingNorthingPair)
        {
            string[] arr = eastingNorthingPair.Split(',');
            double easting = Convert.ToDouble(arr[0]);
            double northing = Convert.ToDouble(arr[1]);
            return EastingNorthingtoLatLongConversion.GetLongitudeLatitude(northing, easting);
        }
    }
}
