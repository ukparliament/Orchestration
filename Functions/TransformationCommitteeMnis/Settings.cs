namespace Functions.TransformationCommitteeMnis
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
            construct{
                ?s a parl:FormalBody.
            }
            where{
                ?s parl:formalBodyMnisId @formalBodyMnisId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?formalBody a parl:FormalBody;
                parl:formalBodyMnisId ?formalBodyMnisId;
                parl:formalBodyName ?formalBodyName;
                parl:formalBodyStartDate ?formalBodyStartDate;
                parl:formalBodyEndDate ?formalBodyEndDate;
                parl:formalBodyHasHouse ?formalBodyHasHouse;
                parl:formalBodyHasParentFormalBody ?formalBodyHasParentFormalBody;
                parl:formalBodyHasFormalBodyChair ?formalBodyHasFormalBodyChair.
            ?formalBodyHasFormalBodyChair a parl:FormalBodyChair.
        }
        where {
            bind(@subject as ?formalBody)
            ?formalBody parl:formalBodyMnisId ?formalBodyMnisId.
            optional {?formalBody parl:formalBodyName ?formalBodyName}
            optional {?formalBody parl:formalBodyStartDate ?formalBodyStartDate}
            optional {?formalBody parl:formalBodyEndDate ?formalBodyEndDate}
            optional {?formalBody parl:formalBodyHasHouse ?formalBodyHasHouse}
            optional {?formalBody parl:formalBodyHasParentFormalBody ?formalBodyHasParentFormalBody}
            optional {?formalBody parl:formalBodyHasFormalBodyChair ?formalBodyHasFormalBodyChair}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Committee_Id,StartDate,EndDate,Name,IsCommons,IsLords,ParentCommittee_Id";
        }
    }
}
