using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationConstituencyMnis
{
    public class Transformation : BaseTransformationXml<Settings, XDocument>
    {
        public override BaseResource[] TransformSource(XDocument doc)
        {
            MnisConstituencyGroup mnisConstituency = new MnisConstituencyGroup();
            XElement constituencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            mnisConstituency.ConstituencyGroupMnisId = constituencyElement.Element(d + "Constituency_Id").GetText();
            mnisConstituency.ConstituencyGroupName = constituencyElement.Element(d + "Name").GetText();
            mnisConstituency.ConstituencyGroupStartDate = constituencyElement.Element(d + "StartDate").GetDate();
            HouseSeat houseSeat = generateHouseSeat();
            mnisConstituency.ConstituencyGroupHasHouseSeat = new HouseSeat[] { houseSeat };
            OnsConstituencyGroup onsConstituencyGroup = new OnsConstituencyGroup();
            onsConstituencyGroup.ConstituencyGroupOnsCode = constituencyElement.Element(d + "ONSCode").GetText();
            PastConstituencyGroup pastConstituencyGroup = new PastConstituencyGroup();
            pastConstituencyGroup.ConstituencyGroupEndDate = constituencyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { mnisConstituency, onsConstituencyGroup, pastConstituencyGroup };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string constituencyGroupMnisId = deserializedSource.OfType<MnisConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupMnisId;
            string constituencyGroupOnsCode = deserializedSource.OfType<OnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode ?? string.Empty;

            return new Dictionary<string, INode>()
            {
                { "constituencyGroupMnisId", SparqlConstructor.GetNode(constituencyGroupMnisId) },
                {"constituencyGroupOnsCode", SparqlConstructor.GetNode(constituencyGroupOnsCode) }
            };
        }

        public override Dictionary<string, INode> GetKeysForTarget(BaseResource[] deserializedSource)
        {
            return new Dictionary<string, INode>()
            {
                {"houseOfCommonsId", SparqlConstructor.GetNode(new Uri(HouseOfCommonsId)) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            MnisConstituencyGroup constituency = source.OfType<MnisConstituencyGroup>().SingleOrDefault();
            HouseSeat houseSeat = target.OfType<HouseSeat>().SingleOrDefault();
            if ((constituency.ConstituencyGroupHasHouseSeat != null) && (constituency.ConstituencyGroupHasHouseSeat.Any()) &&
                (houseSeat != null))
                constituency.ConstituencyGroupHasHouseSeat.SingleOrDefault().Id = houseSeat.Id;

            source.OfType<MnisConstituencyGroup>().SingleOrDefault().Id = subjectUri;
            source.OfType<OnsConstituencyGroup>().SingleOrDefault().Id = subjectUri;
            source.OfType<PastConstituencyGroup>().SingleOrDefault().Id = subjectUri;
            return source;
        }

        private HouseSeat generateHouseSeat()
        {
            HouseSeat houseSeat = new HouseSeat();
            houseSeat.Id = GenerateNewId();
            houseSeat.HouseSeatHasHouse = new House()
            {
                Id = new Uri(HouseOfCommonsId)
            };
            return houseSeat;
        }
    }
}
