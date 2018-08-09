namespace Functions.TransformationProcedureWorkPackagedPreceding
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
            ?proposedNegativeStatutoryInstrumentPaper a parl:ProposedNegativeStatutoryInstrumentPaper;
                parl:proposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper ?statutoryInstrumentPaper.
            ?statutoryInstrumentPaper a parl:StatutoryInstrumentPaper.
        }
        where {
            bind(@subject as ?proposedNegativeStatutoryInstrumentPaper)
            ?proposedNegativeStatutoryInstrumentPaper parl:proposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper ?statutoryInstrumentPaper.
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select nsi.TripleStoreId as ProposedNegativeStatutoryInstrument,
	                si.TripleStoreId as StatutoryInstrument, cast(0 as bit) as IsDeleted from ProcedureWorkPackagedThingPreceding p
                join ProcedureWorkPackagedThing nsi on nsi.Id=p.WorkPackagedIsPrecededById
                join ProcedureWorkPackagedThing si on si.Id=p.WorkPackagedIsFollowedById
                where p.Id={dataUrl}
                union
				select PrecededByTripleStoreId as ProposedNegativeStatutoryInstrument,
	                FollowedByTripleStoreId as StatutoryInstrument, cast(1 as bit) as IsDeleted from DeletedProcedureWorkPackagedThingPreceding
                where Id={dataUrl}";
        }
    }
}
