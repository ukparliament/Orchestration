using System;

namespace Functions.TransformationParliamentPeriod
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
            ?parliamentPeriod a parl:ParliamentPeriod;
                parl:parliamentPeriodNumber ?parliamentPeriodNumber;
                parl:wikidataThingHasEquivalentWikidataResource ?wikidataResource;
                parl:parliamentPeriodHasImmediatelyPreviousParliamentPeriod ?parliamentPeriodHasImmediatelyPreviousParliamentPeriod;
                parl:parliamentPeriodHasImmediatelyFollowingParliamentPeriod ?parliamentPeriodHasImmediatelyFollowingParliamentPeriod;
                parl:parliamentPeriodStartDate ?parliamentPeriodStartDate;
                parl:parliamentPeriodEndDate ?parliamentPeriodEndDate.
            ?parliamentPeriodHasImmediatelyPreviousParliamentPeriod a parl:ParliamentPeriod.
            ?parliamentPeriodHasImmediatelyFollowingParliamentPeriod a parl:ParliamentPeriod.
        }
        where {
            bind(@subject as ?parliamentPeriod)
            optional {?parliamentPeriod parl:parliamentPeriodNumber ?parliamentPeriodNumber}
            optional {?parliamentPeriod parl:wikidataThingHasEquivalentWikidataResource ?wikidataResource}
            optional {?parliamentPeriod parl:parliamentPeriodHasImmediatelyPreviousParliamentPeriod ?parliamentPeriodHasImmediatelyPreviousParliamentPeriod}
            optional {?parliamentPeriod parl:parliamentPeriodHasImmediatelyFollowingParliamentPeriod ?parliamentPeriodHasImmediatelyFollowingParliamentPeriod}
            optional {?parliamentPeriod parl:parliamentPeriodStartDate ?parliamentPeriodStartDate}
            optional {?parliamentPeriod parl:parliamentPeriodEndDate ?parliamentPeriodEndDate}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "cf5e06e7-53b8-4c44-94d5-b9cd0d3e2a33").Replace("{id}", dataUrl);
        }
    }
}
