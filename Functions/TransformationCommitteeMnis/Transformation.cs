using Functions.IdGenerator;
using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationCommitteeMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
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
                            SubjectUri=parentUri
                        }
                    };
            }
            IPastFormalBody pastFormalBody = new PastFormalBody();
            pastFormalBody.FormalBodyEndDate = formalBodyElement.Element(d + "EndDate").GetDate();

            return new IOntologyInstance[] { formalBody, pastFormalBody };
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
                        SubjectUri = houseOfCommonsUri
                    });
            }
            if (formalBodyElement.Element(d + "IsLords").GetBoolean() == true)
            {
                Uri houseOfCommonsUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
                if (houseOfCommonsUri != null)
                    houses.Add(new House()
                    {
                        SubjectUri = houseOfCommonsUri
                    });
            }

            return houses;
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string formalBodyMnisId = deserializedSource.OfType<IMnisFormalBody>()
                .SingleOrDefault()
                .FormalBodyMnisId;
            return new Dictionary<string, object>()
            {
                { "formalBodyMnisId", formalBodyMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IFormalBody formalBody in source.OfType<IFormalBody>())
                formalBody.SubjectUri = subjectUri;
            IMnisFormalBody mnisFormalBody = source.OfType<IMnisFormalBody>().SingleOrDefault();
            FormalBodyChair targetFormalBodyChair = target.OfType<FormalBodyChair>().SingleOrDefault();
            if ((targetFormalBodyChair != null) && (targetFormalBodyChair.SubjectUri != null))
                mnisFormalBody.FormalBodyHasFormalBodyChair = new FormalBodyChair()
                {
                    SubjectUri = targetFormalBodyChair.SubjectUri
                };
            else
                mnisFormalBody.FormalBodyHasFormalBodyChair = new FormalBodyChair()
                {
                    SubjectUri = new Uri(new IdMaker().MakeId())
                };
            return source.OfType<IFormalBody>().ToArray();
        }

    }
}
