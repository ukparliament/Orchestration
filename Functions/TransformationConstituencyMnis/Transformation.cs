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
            ConstituencyGroup constituency = new ConstituencyGroup();
            XElement constituencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            constituency.ConstituencyGroupMnisId = constituencyElement.Element(d + "Constituency_Id").GetText();
            constituency.ConstituencyGroupName = constituencyElement.Element(d + "Name").GetText();
            constituency.ConstituencyGroupStartDate = constituencyElement.Element(d + "StartDate").GetDate();
            HouseSeat houseSeat = generateHouseSeat();
            constituency.ConstituencyGroupHasHouseSeat = new HouseSeat[] { houseSeat };
            constituency.ConstituencyGroupOnsCode = constituencyElement.Element(d + "ONSCode").GetText();
            constituency.ConstituencyGroupEndDate = constituencyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { constituency };
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string constituencyGroupMnisId = deserializedSource.OfType<ConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupMnisId;
            string constituencyGroupOnsCode = deserializedSource.OfType<ConstituencyGroup>()
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
            ConstituencyGroup constituency = source.OfType<ConstituencyGroup>().SingleOrDefault();
            ConstituencyGroup constituencyGroup = target.OfType<ConstituencyGroup>().SingleOrDefault();
            if ((constituency.ConstituencyGroupHasHouseSeat != null) && (constituency.ConstituencyGroupHasHouseSeat.Any()) &&
                (constituencyGroup != null) && (constituencyGroup.ConstituencyGroupHasHouseSeat!=null))
                constituency.ConstituencyGroupHasHouseSeat.SingleOrDefault().Id = constituencyGroup.ConstituencyGroupHasHouseSeat.SingleOrDefault().Id;

            constituency.Id = subjectUri;
            return new BaseResource[] { constituency };
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
