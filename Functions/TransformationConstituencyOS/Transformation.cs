using Functions.TransformationConstituencyOS.EastingNorthingConversion;
using Parliament.Rdf;
using Parliament.Model;
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
        public override IResource[] TransformSource(string response)
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
            return new IResource[] { constituency };
        }

        public override Dictionary<string, object> GetKeysFromSource(IResource[] deserializedSource)
        {
            string constituencyGroupOnsCode = deserializedSource.OfType<IOnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode;
            return new Dictionary<string, object>()
            {
                { "constituencyGroupOnsCode", constituencyGroupOnsCode }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            IOnsConstituencyGroup constituency = source.OfType<IOnsConstituencyGroup>().SingleOrDefault();
            constituency.Id = subjectUri;
            IConstituencyArea constituencyArea = target.OfType<IConstituencyArea>().SingleOrDefault();
            if ((constituencyArea != null) && (constituency.ConstituencyGroupHasConstituencyArea != null))
                constituency.ConstituencyGroupHasConstituencyArea.Id = constituencyArea.Id;

            return new IResource[] { constituency };
        }

        private IConstituencyArea generateConstituencyArea(RDFDescription description)
        {
            IConstituencyArea constituencyArea = new ConstituencyArea();
            constituencyArea.Id = GenerateNewId();
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
                    if ((i < polygons.Length - 1) && (polygons[i + 1].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                    {
                        i++;
                        while ((i < polygons.Length) && (polygons[i].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
                        {
                            string innerPolygon = generatePolygon(polygons[i].Value);
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
            double[][] enPairs = ring.Split(' ')
                .Select(line => new double[] { Convert.ToDouble(line.Split(',')[0]), Convert.ToDouble(line.Split(',')[1]) })
                .ToArray();
            double[][] transformedENPairs = CoordinateTransformation.TransformEastingNorthing(enPairs);
            double[][] longLatPairs = CoordinateConversion.ConvertToLongitudeLatitude(transformedENPairs);

            string[] longLat = longLatPairs
                .Select(longLatPair => string.Join(" ", string.Format("{0:0.00000000000}", longLatPair[0]), string.Format("{0:0.00000000000}", longLatPair[1])))
                .ToArray();
            string polygon = string.Join(",", longLat);
            return $"({polygon})";
        }

    }
}
