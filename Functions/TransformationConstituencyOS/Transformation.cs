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
                        if ((i < polygons.Length - 1) && (polygons[i+1].Parent.Parent.Name.LocalName == "innerBoundaryIs"))
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
                    i++;
                }
                constituencyArea.ConstituencyAreaExtent = areas;
            }
            return constituencyArea;
        }

        private string generatePolygon(string ring)
        {
            string result = string.Join(",", ring.Split(' ').Select(ne => convertEastingNorthingtoLongLat(ne)));
            return $"({result})";
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
