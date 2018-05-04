using Newtonsoft.Json;
using Parliament.Rdf;
using Parliament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationConstituencyOSNI
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IResource[] TransformSource(string response)
        {
            Rootobject sourceConstituency = JsonConvert.DeserializeObject<Rootobject>(response);
            IOnsConstituencyGroup constituency = new OnsConstituencyGroup();
            if ((sourceConstituency != null) && (sourceConstituency.features != null) && (sourceConstituency.features.Any()) && (sourceConstituency.features.SingleOrDefault().attributes != null))
            {
                Feature feature = sourceConstituency.features.SingleOrDefault();
                constituency.ConstituencyGroupOnsCode = feature.attributes.PC_ID;
                if ((feature.geometry != null) && (feature.geometry.rings != null) && (feature.geometry.rings.Any()))
                    constituency.ConstituencyGroupHasConstituencyArea = generateConstituencyArea(feature.geometry.rings);
            }
            return new IResource[] { constituency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string constituencyGroupOnsCode = deserializedSource.OfType<IOnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode;
            return new Dictionary<string, INode>()
            {
                { "constituencyGroupOnsCode", SparqlConstructor.GetNode(constituencyGroupOnsCode) }
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

        private IConstituencyArea generateConstituencyArea(decimal[][][] rings)
        {
            IConstituencyArea constituencyArea = new ConstituencyArea();
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
