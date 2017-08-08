using Functions.Transformation;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationConstituencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        protected XNamespace atom = "http://www.w3.org/2005/Atom";
        protected XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        protected XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IBaseOntology[] TransformSource(string response)
        {
            IMnisConstituencyGroup mnisConstituency = new MnisConstituencyGroup();
            XDocument doc = XDocument.Parse(response);
            XElement constituencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            mnisConstituency.ConstituencyGroupMnisId = constituencyElement.Element(d + "Constituency_Id").GetText();
            mnisConstituency.ConstituencyGroupName = constituencyElement.Element(d + "Name").GetText();
            mnisConstituency.ConstituencyGroupStartDate = constituencyElement.Element(d + "StartDate").GetDate();
            IHouseSeat houseSeat = generateHouseSeat();
            mnisConstituency.ConstituencyGroupHasHouseSeat = new IHouseSeat[] { houseSeat };
            IOnsConstituencyGroup onsConstituencyGroup = new OnsConstituencyGroup();
            onsConstituencyGroup.ConstituencyGroupOnsCode = constituencyElement.Element(d + "ONSCode").GetText();
            IPastConstituencyGroup pastConstituencyGroup = new PastConstituencyGroup();
            pastConstituencyGroup.ConstituencyGroupEndDate = constituencyElement.Element(d + "EndDate").GetDate();

            return new IBaseOntology[] { mnisConstituency, onsConstituencyGroup, pastConstituencyGroup };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string constituencyGroupMnisId = deserializedSource.OfType<IMnisConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupMnisId;
            string constituencyGroupOnsCode = deserializedSource.OfType<IOnsConstituencyGroup>()
                .SingleOrDefault()
                .ConstituencyGroupOnsCode;
            return new Dictionary<string, object>()
            {
                { "constituencyGroupMnisId", constituencyGroupMnisId },
                {"constituencyGroupOnsCode", constituencyGroupOnsCode }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IMnisConstituencyGroup constituency = source.OfType<IMnisConstituencyGroup>().SingleOrDefault();
            IHouseSeat houseSeat= target.OfType<IHouseSeat>().SingleOrDefault();
            if ((constituency.ConstituencyGroupHasHouseSeat != null) && (constituency.ConstituencyGroupHasHouseSeat.Any()) &&
                (houseSeat != null))
                constituency.ConstituencyGroupHasHouseSeat.SingleOrDefault().SubjectUri = houseSeat.SubjectUri;
            
            foreach (IConstituencyGroup constituencyGroup in source.OfType<IConstituencyGroup>())
                constituencyGroup.SubjectUri = subjectUri;
            return source.OfType<IConstituencyGroup>().ToArray();
        }


        private IHouseSeat generateHouseSeat()
        {
            IHouseSeat houseSeat = new HouseSeat();
            houseSeat.SubjectUri = GenerateNewId();
            Uri houseUri = IdRetrieval.GetSubject("houseName", "House of Commons", false, logger);
            if (houseUri == null)
                logger.Warning("No house found");
            else
                houseSeat.HouseSeatHasHouse = new House()
                {
                    SubjectUri = houseUri
                };
            return houseSeat;
        }
    }
}
