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
                string[] polygons = XDocument.Parse(xmlPolygon)
                    .Descendants("coordinates")
                    .Select(c => c.Value)
                    .ToArray();
                constituencyArea.ConstituencyAreaExtent = generateConstituencyAreaExtent(polygons);                
            }
            return constituencyArea;
        }

        private string[] generateConstituencyAreaExtent(string[] rings)
        {
            List<string> areas = new List<string>();
            foreach (string ring in rings)
            {
                string polygon = string.Join(",", ring.Split(' ').Select(ne => convertNEtoLongLat(ne)));
                areas.Add($"Polygon(({polygon}))");
            }
            return areas.ToArray();
        }

        private string convertNEtoLongLat(string nePair)
        {
            string[] arr = nePair.Split(',');
            double northing = Convert.ToDouble(arr[1]);
            double easting = Convert.ToDouble(arr[0]);
            return NEtoLatLongConversion.GetLongitudeLatitude(northing, easting);
        }
    }
}
