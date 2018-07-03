namespace Functions.TransformationConstituencyMnis
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
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

        public string ExistingGraphSparqlCommand
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
                    parl:constituencyGroupEndDate ?constituencyGroupEndDate;
                    parl:constituencyGroupHasHouseSeat ?seat.
                ?seat a parl:HouseSeat;
                    parl:houseSeatHasHouse ?house.
                ?house a parl:House.
            }
            where {
                bind(@subject as ?constituencyGroup)
                bind(@houseOfCommonsId as ?house)
                ?constituencyGroup parl:constituencyGroupMnisId ?constituencyGroupMnisId.
                optional {?constituencyGroup parl:constituencyGroupOnsCode ?constituencyGroupOnsCode}
                optional {?constituencyGroup parl:constituencyGroupName ?constituencyGroupName}
                optional {?constituencyGroup parl:constituencyGroupStartDate ?constituencyGroupStartDate}
                optional {?constituencyGroup parl:constituencyGroupEndDate ?constituencyGroupEndDate}
                optional {
                    ?constituencyGroup parl:constituencyGroupHasHouseSeat ?seat.
                    optional {
                        ?seat parl:houseSeatHasHouse ?house.
                    }
                }
            }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Constituency_Id,Name,StartDate,EndDate,ONSCode";
        }
    }
}