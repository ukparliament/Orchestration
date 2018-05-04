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
            optional {?workPackageableThing parl:workPackageableThingComingIntoForceDate ?workPackageableThingComingIntoForceDate}
            optional {?workPackageableThing parl:workPackageableThingComingIntoForceNote ?workPackageableThingComingIntoForceNote}
            optional {?workPackageableThing parl:workPackageableThingTimeLimitForObjectionEndDate ?workPackageableThingTimeLimitForObjectionEndDate}
            optional {?workPackageableThing parl:workPackageableThingHasWorkPackageableThingWebLink ?workPackageableThingHasWorkPackageableThingWebLink}
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "25101afd-6e12-4e13-b981-e3b2a12f112e").Replace("{id}", dataUrl);
        }
    }
}
