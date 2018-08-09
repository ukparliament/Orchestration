namespace Functions.TransformationProcedureWorkPackagedThing
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
            ?workPackagedThing a parl:WorkPackagedThing;
                ?workPackagedThingType ?workPackagedThingName;
                parl:statutoryInstrumentPaperNumber ?statutoryInstrumentPaperNumber;
                parl:statutoryInstrumentPaperPrefix ?statutoryInstrumentPaperPrefix;
                parl:statutoryInstrumentPaperYear ?statutoryInstrumentPaperYear;
                parl:statutoryInstrumentPaperComingIntoForceDate ?statutoryInstrumentPaperComingIntoForceDate;
                parl:statutoryInstrumentPaperComingIntoForceNote ?statutoryInstrumentPaperComingIntoForceNote;
                parl:statutoryInstrumentPaperMadeDate ?statutoryInstrumentPaperMadeDate;
                parl:workPackagedThingHasWorkPackagedThingWebLink ?workPackagedThingHasWorkPackagedThingWebLink.
            ?workPackagedThingHasWorkPackagedThingWebLink a parl:WorkPackagedThingWebLink.
        }
        where {
            bind(@subject as ?workPackagedThing)
            optional {
                ?workPackagedThing parl:statutoryInstrumentPaperName ?workPackagedThingName.
                bind(parl:statutoryInstrumentPaperName as ?workPackagedThingType)
            }
            optional {
                ?workPackagedThing parl:proposedNegativeStatutoryInstrumentPaperName ?workPackagedThingName.
                bind(parl:proposedNegativeStatutoryInstrumentPaperName as ?workPackagedThingType)
            }
            optional {?workPackagedThing parl:statutoryInstrumentPaperNumber ?statutoryInstrumentPaperNumber}
            optional {?workPackagedThing parl:statutoryInstrumentPaperPrefix ?statutoryInstrumentPaperPrefix}
            optional {?workPackagedThing parl:statutoryInstrumentPaperYear ?statutoryInstrumentPaperYear}
            optional {?workPackagedThing parl:statutoryInstrumentPaperComingIntoForceDate ?statutoryInstrumentPaperComingIntoForceDate}
            optional {?workPackagedThing parl:statutoryInstrumentPaperComingIntoForceNote ?statutoryInstrumentPaperComingIntoForceNote}
            optional {?workPackagedThing parl:statutoryInstrumentPaperMadeDate ?statutoryInstrumentPaperMadeDate}
            optional {?workPackagedThing parl:workPackagedThingHasWorkPackagedThingWebLink ?workPackagedThingHasWorkPackagedThingWebLink}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select wp.TripleStoreId, coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName) as WorkPackagedThingName,
	                si.StatutoryInstrumentNumber, si.StatutoryInstrumentNumberPrefix, si.StatutoryInstrumentNumberYear,
	                si.ComingIntoForceNote, si.ComingIntoForceDate, si.MadeDate, wp.WebLink, cast (0 as bit) as IsDeleted,
	                cast(iif(si.ProcedureStatutoryInstrumentName is null,0,1) as bit) as IsStatutoryInstrument
                from ProcedureWorkPackagedThing wp
                left join ProcedureStatutoryInstrument si on si.Id=wp.Id
                left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
                where wp.Id={dataUrl}
                union
				select wp.TripleStoreId, null as WorkPackagedThingName,
	                null as StatutoryInstrumentNumber, null as StatutoryInstrumentNumberPrefix, null as StatutoryInstrumentNumberYear,
	                null as ComingIntoForceNote, null as ComingIntoForceDate, null as MadeDate, null as WebLink, cast (1 as bit) as IsDeleted,
	                null as IsStatutoryInstrument
                from DeletedProcedureWorkPackagedThing wp
                where wp.Id={dataUrl}";
        }
    }
}
