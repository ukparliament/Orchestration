using System;

namespace Functions.TransformationProcedureWorkPackageableThing
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
            ?workPackageableThing a parl:WorkPackageableThing;
                parl:workPackageableThingName ?workPackageableThingName;
                parl:statutoryInstrumentNumber ?statutoryInstrumentNumber;
                parl:statutoryInstrumentNumberPrefix ?statutoryInstrumentNumberPrefix;
                parl:statutoryInstrumentNumberYear ?statutoryInstrumentNumberYear;
                parl:workPackageableThingComingIntoForceDate ?workPackageableThingComingIntoForceDate;
                parl:workPackageableThingComingIntoForceNote ?workPackageableThingComingIntoForceNote;
                parl:workPackageableThingTimeLimitForObjectionEndDate ?workPackageableThingTimeLimitForObjectionEndDate;
                parl:workPackageableThingHasWorkPackageableThingWebLink ?workPackageableThingHasWorkPackageableThingWebLink.
            ?workPackageableThingHasWorkPackageableThingWebLink a parl:WorkPackageableThingWebLink.
        }
        where {
            bind(@subject as ?workPackageableThing)
            ?workPackageableThing parl:workPackageableThingName ?workPackageableThingName.
            optional {?workPackageableThing parl:statutoryInstrumentNumber ?statutoryInstrumentNumber}
            optional {?workPackageableThing parl:statutoryInstrumentNumberPrefix ?statutoryInstrumentNumberPrefix}
            optional {?workPackageableThing parl:statutoryInstrumentNumberYear ?statutoryInstrumentNumberYear}
            optional {?workPackageableThing parl:workPackageableThingComingIntoForceDate ?workPackageableThingComingIntoForceDate}
            optional {?workPackageableThing parl:workPackageableThingComingIntoForceNote ?workPackageableThingComingIntoForceNote}
            optional {?workPackageableThing parl:workPackageableThingTimeLimitForObjectionEndDate ?workPackageableThingTimeLimitForObjectionEndDate}
            optional {?workPackageableThing parl:workPackageableThingHasWorkPackageableThingWebLink ?workPackageableThingHasWorkPackageableThingWebLink}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select wpt.TripleStoreId, wpt.ProcedureWorkPackageableThingName, wpt.StatutoryInstrumentNumber,
                    wpt.StatutoryInstrumentNumberPrefix, wpt.StatutoryInstrumentNumberYear, wpt.ComingIntoForceDate,
                    wpt.ComingIntoForceNote, wpt.TimeLimitForObjectionEndDate, wpt.WebLink, wpt.IsDeleted from ProcedureWorkPackageableThing wpt
                where wpt.Id={dataUrl}";
        }
    }
}
