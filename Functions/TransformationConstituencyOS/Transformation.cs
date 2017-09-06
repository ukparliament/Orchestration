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
                int i = 0;
                List<string> areas = new List<string>();
                while (i < polygons.Length)
                {
                    if (polygons[i].Parent.Parent.Name.LocalName == "outerBoundaryIs")
                    {
                        string polygon = generatePolygon(polygons[i].Value);
                        if (string.IsNullOrWhiteSpace(polygon) == false)
                        {
                            if ((i < polygons.Length - 1) && (polygons[i + 1].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                            {
                                i++;
                                while ((i < polygons.Length) && (polygons[i].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                                {
                                    polygon = $"{polygon},{generatePolygon(polygons[i].Value)}";
                                    i++;
                                }
                            }
                            areas.Add($"Polygon({polygon})");
                        }
                    }
                    i++;
                }
                constituencyArea.ConstituencyAreaExtent = areas;
            }
            return constituencyArea;
        }

        private string generatePolygon(string ring)
        {
            string inputFile = Path.GetTempFileName();
            File.WriteAllLines(inputFile, ring.Split(' '));
            string polygon = convertEastingNorthingtoLongLat(inputFile);
            File.Delete(inputFile);
            return polygon;
        }

        private string convertEastingNorthingtoLongLat(string inputFile)
        {
            string[] conversionOutput = null;
            try
            {
                string outputFile = Path.GetTempFileName();
                string[] arguments =
                    {
                        "\"aaaaa\\TransformationSettings.xml\"",
                        $"\"{inputFile}\"",
                        $"\"{outputFile}\""
                    };
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "aaaaa\\giqtrans.exe";
                process.StartInfo.Arguments = string.Join(" ", arguments);
                process.Start();
                while (process.HasExited == false)
                { }
                conversionOutput = File.ReadAllLines(outputFile);
                File.Delete(outputFile);
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
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
