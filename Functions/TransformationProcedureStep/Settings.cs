using System;

namespace Functions.TransformationProcedureStep
{
    public class Settings : ITransformationSettings
    {
        public string AcceptHeader
        {
            get
            {
                return string.Empty;
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return null;
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
            ?procedureStep a parl:ProcedureStep;
        	    parl:procedureStepName ?procedureStepName;
                parl:procedureStepDescription ?procedureStepDescription;
                parl:procedureStepScopeNote ?procedureStepScopeNote;
                parl:procedureStepLinkNote ?procedureStepLinkNote;
                parl:procedureStepDateNote ?procedureStepDateNote;
                parl:procedureStepHasHouse ?procedureStepHasHouse.
            ?procedureStepHasHouse a parl:House.
        }
        where {
            bind(@subject as ?procedureStep)
            ?procedureStep parl:procedureStepName ?procedureStepName.
            optional {?procedureStep parl:procedureStepDescription ?procedureStepDescription}
    optional {?procedureStep parl:procedureStepScopeNote ?procedureStepScopeNote}
    optional {?procedureStep parl:procedureStepLinkNote ?procedureStepLinkNote}
    optional {?procedureStep parl:procedureStepDateNote ?procedureStepDateNote}
            optional {?procedureStep parl:procedureStepHasHouse ?procedureStepHasHouse}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select s.TripleStoreId, s.ProcedureStepName, 
                    s.ProcedureStepDescription, 
                    s.ProcedureStepScopeNote,
                    s.ProcedureStepLinkNote,
                    s.ProcedureStepDateNote,
                    cast(0 as bit) as IsDeleted from ProcedureStep s
                where s.Id={dataUrl}
                union
                select s.TripleStoreId, null as ProcedureStepName, 
                    null as ProcedureStepDescription, 
                    null as ProcedureStepScopeNote,
                    null as ProcedureStepLinkNote,
                    null as ProcedureStepDateNote,
                    cast(1 as bit) as IsDeleted from DeletedProcedureStep s
                where s.Id={dataUrl};
                select h.TripleStoreId as House from ProcedureStepHouse s
                join House h on h.Id=s.HouseId
                where s.ProcedureStepId={dataUrl}";
        }
    }
}
