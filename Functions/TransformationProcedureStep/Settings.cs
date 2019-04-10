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
                parl:procedureStepHasHouse ?procedureStepHasHouse;
                parl:procedureStepIsCommonlyActualisedAlongsideProcedureStep ?procedureStepIsCommonlyActualisedAlongsideProcedureStep;
                parl:procedureStepHasProcedureStepPublication ?procedureStepHasProcedureStepPublication.
            ?procedureStepHasHouse a parl:House.
            ?procedureStepCommonlyActualisedAlongsideProcedureStep a parl:ProcedureStep.
            ?procedureStepHasProcedureStepPublication a parl:ProcedureStepPublication;
                parl:procedureStepPublicationName ?procedureStepPublicationName;
                parl:procedureStepPublicationUrl ?procedureStepPublicationUrl.
        }
        where {
            bind(@subject as ?procedureStep)
            ?procedureStep parl:procedureStepName ?procedureStepName.
            optional {?procedureStep parl:procedureStepDescription ?procedureStepDescription}
            optional {?procedureStep parl:procedureStepScopeNote ?procedureStepScopeNote}
            optional {?procedureStep parl:procedureStepLinkNote ?procedureStepLinkNote}
            optional {?procedureStep parl:procedureStepDateNote ?procedureStepDateNote}
            optional {?procedureStep parl:procedureStepIsCommonlyActualisedAlongsideProcedureStep ?procedureStepIsCommonlyActualisedAlongsideProcedureStep} 
            optional {?procedureStep parl:procedureStepHasHouse ?procedureStepHasHouse}
            optional {
                ?procedureStep parl:procedureStepHasProcedureStepPublication ?procedureStepHasProcedureStepPublication.
                optional {?procedureStepHasProcedureStepPublication parl:procedureStepPublicationName ?procedureStepPublicationName}
                optional {?procedureStepHasProcedureStepPublication parl:procedureStepPublicationUrl ?procedureStepPublicationUrl}
            }
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
                    s1.TripleStoreId as LinkedTripleStoreId,
                    cast(0 as bit) as IsDeleted,
                    psp.TripleStoreId as PubTripleStoreId,
                    psp.PublicationName,
                    psp.PublicationUrl
                from ProcedureStep s
                left join ProcedureStep s1 on s1.Id = s.CommonlyActualisedAlongsideProcedureStepId
                left join ProcedureStepPublication psp on psp.Id = s.ProcedureStepPublicationId
                where s.Id={dataUrl}
                union
                select s.TripleStoreId, null as ProcedureStepName, 
                    null as ProcedureStepDescription, 
                    null as ProcedureStepScopeNote,
                    null as ProcedureStepLinkNote,
                    null as ProcedureStepDateNote,
                    null as LinkedTripleStoreId,
                    cast(1 as bit) as IsDeleted,
                    null as PubTripleStoreId,
                    null as PublicationName,
                    null as PublicationUrl
                from DeletedProcedureStep s
                where s.Id={dataUrl};
                select h.TripleStoreId as House from ProcedureStepHouse s
                join House h on h.Id=s.HouseId
                where s.ProcedureStepId={dataUrl};
                ";
        }
    }
}
