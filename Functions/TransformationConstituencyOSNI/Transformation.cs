using Newtonsoft.Json;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationConstituencyOSNI
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
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

        private IConstituencyArea generateConstituencyArea(decimal[][][] rings)
        {
            IConstituencyArea constituencyArea = new ConstituencyArea();
            constituencyArea.SubjectUri = GenerateNewId();
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
