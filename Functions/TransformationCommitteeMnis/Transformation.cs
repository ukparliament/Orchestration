using Functions.IdGenerator;
using Parliament.Rdf;
using Parliament.Model;
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

        public override IResource[] TransformSource(string response)
        {
            IMnisFormalBody formalBody = new MnisFormalBody();
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
                    formalBody.FormalBodyHasParentFormalBody = new List<IFormalBody>
                    {
                        new FormalBody()
                        {
                            Id=parentUri
                        }
                    };
            }
            IPastFormalBody pastFormalBody = new PastFormalBody();
            pastFormalBody.FormalBodyEndDate = formalBodyElement.Element(d + "EndDate").GetDate();

            return new IResource[] { formalBody, pastFormalBody };
        }

        private List<IHouse> generateHouseMembership(XElement formalBodyElement)
        {
            List<IHouse> houses = new List<IHouse>();
            if (formalBodyElement.Element(d + "IsCommons").GetBoolean() == true)
            {
                Uri houseOfCommonsUri = IdRetrieval.GetSubject("houseName", "House of Commons", false, logger);
                if (houseOfCommonsUri != null)
                    houses.Add(new House()
                    {
                        Id = houseOfCommonsUri
                    });
            }
            if (formalBodyElement.Element(d + "IsLords").GetBoolean() == true)
            {
                Uri houseOfCommonsUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
                if (houseOfCommonsUri != null)
                    houses.Add(new House()
                    {
                        Id = houseOfCommonsUri
                    });
            }

            return houses;
        }

        public override Dictionary<string, INode> GetKeysFromSource(IResource[] deserializedSource)
        {
            string formalBodyMnisId = deserializedSource.OfType<IMnisFormalBody>()
                .SingleOrDefault()
                .FormalBodyMnisId;
            return new Dictionary<string, INode>()
            {
                { "formalBodyMnisId", SparqlConstructor.GetNode(formalBodyMnisId) }
            };
        }

        public override IResource[] SynchronizeIds(IResource[] source, Uri subjectUri, IResource[] target)
        {
            foreach (IFormalBody formalBody in source.OfType<IFormalBody>())
                formalBody.Id = subjectUri;
            IMnisFormalBody mnisFormalBody = source.OfType<IMnisFormalBody>().SingleOrDefault();
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
            return source.OfType<IFormalBody>().ToArray();
        }

    }
}
