﻿using System;

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
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "FFCFA882-EE6D-4467-B146-68CD62924256").Replace("{id}", dataUrl);
        }
    }
}
