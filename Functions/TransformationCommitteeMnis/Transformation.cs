using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;

namespace Functions.TransformationCommitteeMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override BaseResource[] TransformSource(string response)
        {
            MnisFormalBody formalBody = new MnisFormalBody();
            XDocument doc = XDocument.Parse(response);
            XElement formalBodyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            formalBody.FormalBodyMnisId = formalBodyElement.Element(d + "Committee_Id").GetText();
            formalBody.FormalBodyName = formalBodyElement.Element(d + "Name").GetText();
            formalBody.FormalBodyStartDate = formalBodyElement.Element(d + "StartDate").GetDate();
            formalBody.FormalBodyHasHouse = generateHouseMembership(formalBodyElement);
            string parentId = formalBodyElement.Element(d + "ParentCommittee_Id").GetText();
            if (string.IsNullOrWhiteSpace(parentId) == false)
            {
                Uri parentUri = IdRetrieval.GetSubject("formalBodyMnisId", parentId, false, logger);
                if (parentId != null)
                    formalBody.FormalBodyHasParentFormalBody = new List<FormalBody>
                    {
                        new FormalBody()
                        {
                            Id=parentUri
                        }
                    };
            }
            PastFormalBody pastFormalBody = new PastFormalBody();
            pastFormalBody.FormalBodyEndDate = formalBodyElement.Element(d + "EndDate").GetDate();

            return new BaseResource[] { formalBody, pastFormalBody };
        }

        private List<House> generateHouseMembership(XElement formalBodyElement)
        {
            List<House> houses = new List<House>();
            if (formalBodyElement.Element(d + "IsCommons").GetBoolean() == true)
                houses.Add(new House()
                {
                    Id = new Uri(HouseOfCommonsId)
                });
            if (formalBodyElement.Element(d + "IsLords").GetBoolean() == true)
                houses.Add(new House()
                {
                    Id = new Uri(HouseOfLordsId)
                });

            return houses;
        }

        public override Dictionary<string, INode> GetKeysFromSource(BaseResource[] deserializedSource)
        {
            string formalBodyMnisId = deserializedSource.OfType<MnisFormalBody>()
                .SingleOrDefault()
                .FormalBodyMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyMnisId", SparqlConstructor.GetNode(formalBodyMnisId) }
            };
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            foreach (BaseResource formalBody in source)
                formalBody.Id = subjectUri;
            MnisFormalBody mnisFormalBody = source.OfType<MnisFormalBody>().SingleOrDefault();
            FormalBodyChair targetFormalBodyChair = target.OfType<FormalBodyChair>().SingleOrDefault();
            if ((targetFormalBodyChair != null) && (targetFormalBodyChair.Id != null))
                mnisFormalBody.FormalBodyHasFormalBodyChair = new FormalBodyChair()
                {
                    Id = targetFormalBodyChair.Id
                };
            else
                mnisFormalBody.FormalBodyHasFormalBodyChair = new FormalBodyChair()
                {
                    Id = GenerateNewId()
                };
            return source;
        }

    }
}
