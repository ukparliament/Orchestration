using Newtonsoft.Json;
using Parliament.Rdf.Serialization;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationConstituencyOSNI
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override BaseResource[] TransformSource(string response)
        {
            Rootobject sourceConstituency = JsonConvert.DeserializeObject<Rootobject>(response);
            OnsConstituencyGroup constituency = new OnsConstituencyGroup();
            if ((sourceConstituency != null) && (sourceConstituency.features != null) && (sourceConstituency.features.Any()) && (sourceConstituency.features.SingleOrDefault().attributes != null))
            {
                Feature feature = sourceConstituency.features.SingleOrDefault();
                constituency.ConstituencyGroupOnsCode = feature.attributes.PC_ID;
                if ((feature.geometry != null) && (feature.geometry.rings != null) && (feature.geometry.rings.Any()))
                    constituency.ConstituencyGroupHasConstituencyArea = generateConstituencyArea(feature.geometry.rings);
            }
            return new BaseResource[] { constituency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string constituencyGroupOnsCode = deserializedSource.OfType<OnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode;
            return new Dictionary<string, INode>()
            {
                { "constituencyGroupOnsCode", SparqlConstructor.GetNode(constituencyGroupOnsCode) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            OnsConstituencyGroup constituency = source.OfType<OnsConstituencyGroup>().SingleOrDefault();
            constituency.Id = subjectUri;
            ConstituencyArea constituencyArea = target.OfType<ConstituencyArea>().SingleOrDefault();
            if ((constituencyArea != null) && (constituency.ConstituencyGroupHasConstituencyArea != null))
                constituency.ConstituencyGroupHasConstituencyArea.Id = constituencyArea.Id;

            return new BaseResource[] { constituency };
        }

        private ConstituencyArea generateConstituencyArea(decimal[][][] rings)
        {
            ConstituencyArea constituencyArea = new ConstituencyArea();
            constituencyArea.Id = GenerateNewId();
            constituencyArea.ConstituencyAreaExtent = generateConstituencyAreaExtent(rings);
            return constituencyArea;
        }

        private string[] generateConstituencyAreaExtent(decimal[][][] rings)
        {
            List<string> areas = new List<string>();
            foreach (decimal[][] ring in rings)
            {
                string polygon = string.Join(",", ring.Select(longLat => $"{longLat[0]} {longLat[1]}"));
                areas.Add($"Polygon(({polygon}))");
            }
            return areas.ToArray();
        }

    }
}
