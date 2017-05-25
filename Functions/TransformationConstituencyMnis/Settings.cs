using System.Collections.Generic;
using System.Xml;

namespace Functions.Transformation
{
    public class Settings : ITransformationSettings
    {
        public string OperationName
        {
            get
            {
                return "TransformationConstituencyMnis";
            }
        }

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public XmlNamespaceManager SourceXmlNamespaceManager
        {
            get
            {
                XmlNamespaceManager sourceXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                sourceXmlNamespaceManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                sourceXmlNamespaceManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
                sourceXmlNamespaceManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                return sourceXmlNamespaceManager;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct {
                ?constituencyGroup a parl:ConstituencyGroup.
            }
            where {
                {
                    ?constituencyGroup parl:constituencyGroupMnisId ?constituencyGroupMnisId.
                    filter(?constituencyGroupMnisId=@constituencyGroupMnisId)
                }
                union
                {
                    ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.
                    filter(?constituencyGroupOnsCode=@constituencyGroupOnsCode)
                }
            }
        ";
            }
        }

        public Dictionary<string, string> SubjectRetrievalParameters
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {"constituencyGroupMnisId", "atom:entry/atom:content/m:properties/d:Constituency_Id" },
                    {"constituencyGroupOnsCode", "atom:entry/atom:content/m:properties/d:ONSCode" }
                };
            }
        }

        public string ExisitngGraphSparqlCommand
        {
            get
            {
                return @"
            construct {
        	    ?constituencyGroup a parl:ConstituencyGroup;
                    parl:constituencyGroupMnisId ?constituencyGroupMnisId;
                    parl:constituencyGroupOnsCode ?constituencyGroupOnsCode;
                    parl:constituencyGroupName ?constituencyGroupName;
                    parl:constituencyGroupStartDate ?constituencyGroupStartDate;
                    parl:constituencyGroupEndDate ?constituencyGroupEndDate.
                ?seat a parl:HouseSeat;
                    parl:houseSeatHasHouse ?house;
                    parl:houseSeatHasConstituencyGroup ?constituencyGroup.
            }
            where {
                bind(@subject as ?constituencyGroup)
                ?constituencyGroup a parl:ConstituencyGroup;
                    parl:constituencyGroupMnisId ?constituencyGroupMnisId.
                optional { ?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode.}
                optional {?constituencyGroup parl:constituencyGroupName ?constituencyGroupName}
                optional {?constituencyGroup parl:constituencyGroupStartDate ?constituencyGroupStartDate}
                optional {?constituencyGroup parl:constituencyGroupEndDate ?constituencyGroupEndDate}
                ?seat a parl:HouseSeat;
                    parl:houseSeatHasHouse ?house;
                    parl:houseSeatHasConstituencyGroup ?constituencyGroup.
                ?house parl:houseName ""House of Commons"".
            }";
            }
        }

        public Dictionary<string, string> ExistingGraphSparqlParameters
        {
            get
            {
                return null;
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Constituency_Id,Name,StartDate,EndDate,ONSCode";
        }
    }
}