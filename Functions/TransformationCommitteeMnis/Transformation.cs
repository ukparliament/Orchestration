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

        public override IBaseOntology[] TransformSource(string response)
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
            IPastFormalBody pastFormalBody = new PastFormalBody();
            pastFormalBody.FormalBodyEndDate = formalBodyElement.Element(d + "EndDate").GetDate();

            return new IBaseOntology[] { formalBody, pastFormalBody };
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

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string formalBodyMnisId = deserializedSource.OfType<IMnisFormalBody>()
                .SingleOrDefault()
                .FormalBodyMnisId;
            return new Dictionary<string, object>()
            {
                { "formalBodyMnisId", formalBodyMnisId }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            foreach (IFormalBody formalBody in source.OfType<IFormalBody>())
                formalBody.SubjectUri = subjectUri;
            return source.OfType<IFormalBody>().ToArray();
        }

    }
}
