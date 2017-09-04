using Parliament.Ontology.Base;
using Parliament.Ontology.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Functions.TransformationLordsTypeMnis
{
    public class Transformation : BaseTransformation<Settings>
    {
        public override IBaseOntology[] TransformSource(string response)
        {
            IMnisHouseIncumbencyType incumbencyType = new MnisHouseIncumbencyType();
            XDocument doc = XDocument.Parse(response);
            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            XElement element = doc.Descendants(m + "properties").SingleOrDefault();

            incumbencyType.HouseIncumbencyTypeMnisId = element.Element(d + "LordsMembershipType_Id").GetText();
            incumbencyType.HouseIncumbencyTypeName = element.Element(d + "Name").GetText();

            return new IBaseOntology[] { incumbencyType };
        }

        public override Dictionary<string, object> GetKeysFromSource(IBaseOntology[] deserializedSource)
        {
            string houseIncumbencyTypeMnisId = deserializedSource.OfType<IMnisHouseIncumbencyType>()
                .SingleOrDefault()
                .HouseIncumbencyTypeMnisId;
            return new Dictionary<string, object>()
            {
                { "houseIncumbencyTypeMnisId", houseIncumbencyTypeMnisId }
            };
        }

        public override IBaseOntology[] SynchronizeIds(IBaseOntology[] source, Uri subjectUri, IBaseOntology[] target)
        {
            IMnisHouseIncumbencyType incumbencyType = source.OfType<IMnisHouseIncumbencyType>().SingleOrDefault();
            incumbencyType.SubjectUri = subjectUri;

            return new IBaseOntology[] { incumbencyType };
        }

    }
}
