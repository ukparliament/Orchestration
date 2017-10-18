using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Functions.TransformationHouseIncumbencyMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        private XNamespace atom = "http://www.w3.org/2005/Atom";
        private XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        public override IOntologyInstance[] TransformSource(string response)
        {
            Uri houseUri = IdRetrieval.GetSubject("houseName", "House of Lords", false, logger);
            XDocument doc = XDocument.Parse(response);
            XElement lordIncumbencyElement = doc.Element(atom + "entry")
                .Element(atom + "content")
                .Element(m + "properties");

            IPastParliamentaryIncumbency incumbency = new PastParliamentaryIncumbency();
            incumbency.ParliamentaryIncumbencyStartDate = lordIncumbencyElement.Element(d + "StartDate").GetDate();
            incumbency.ParliamentaryIncumbencyEndDate = lordIncumbencyElement.Element(d + "EndDate").GetDate();
            string memberId = lordIncumbencyElement.Element(d + "Member_Id").GetText();
            Uri memberUri = IdRetrieval.GetSubject("personMnisId", memberId, false, logger);
            if (memberUri == null)
            {
                logger.Warning($"No member found for {memberId}");
                return null;
            }
            incumbency.ParliamentaryIncumbencyHasMember = new Member()
            {
                SubjectUri = memberUri
            };
            if ((incumbency.ParliamentaryIncumbencyEndDate.HasValue == false) || (incumbency.ParliamentaryIncumbencyStartDate <= incumbency.ParliamentaryIncumbencyEndDate))
            {
                IMnisHouseIncumbency houseIncumbency = new MnisHouseIncumbency();
                houseIncumbency.HouseIncumbencyMnisId = lordIncumbencyElement.Element(d + "MemberLordsMembershipType_Id").GetText();
                string houseIncumbencyTypeMnisId = lordIncumbencyElement.Element(d + "LordsMembershipType_Id").GetText();
                Uri houseIncumbencyTypeUri = IdRetrieval.GetSubject("houseIncumbencyTypeMnisId", houseIncumbencyTypeMnisId, false, logger);
                if (houseIncumbencyTypeUri == null)
                {
                    logger.Warning($"No incumbency type found for {houseIncumbencyTypeMnisId}");
                    return null;
                }
                houseIncumbency.HouseIncumbencyHasHouseIncumbencyType = new HouseIncumbencyType()
                {
                    SubjectUri = houseIncumbencyTypeUri
                };
                houseIncumbency.HouseIncumbencyHasHouse = new House() { SubjectUri = houseUri };
                return new IOntologyInstance[] { incumbency, houseIncumbency };
            }
            return null;
        }

        public override Dictionary<string, object> GetKeysFromSource(IOntologyInstance[] deserializedSource)
        {
            string houseIncumbencyMnisId = deserializedSource.OfType<IMnisHouseIncumbency>()
                .SingleOrDefault()
                .HouseIncumbencyMnisId;
            return new Dictionary<string, object>()
            {
                { "houseIncumbencyMnisId", houseIncumbencyMnisId }
            };
        }

        public override IOntologyInstance[] SynchronizeIds(IOntologyInstance[] source, Uri subjectUri, IOntologyInstance[] target)
        {
            foreach (IParliamentaryIncumbency incumbency in source.OfType<IParliamentaryIncumbency>())
                incumbency.SubjectUri = subjectUri;
            return source.OfType<IParliamentaryIncumbency>().ToArray();
        }

    }
}