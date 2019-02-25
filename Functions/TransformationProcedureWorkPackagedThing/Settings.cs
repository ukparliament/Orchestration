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
                parl:treatyCommandPaperNumber ?treatyCommandPaperNumber;
                parl:treatyCommandPaperPrefix ?treatyCommandPaperPrefix;
                parl:statutoryInstrumentPaperYear ?statutoryInstrumentPaperYear;
                parl:statutoryInstrumentPaperComingIntoForceDate ?statutoryInstrumentPaperComingIntoForceDate;
                parl:statutoryInstrumentPaperComingIntoForceNote ?statutoryInstrumentPaperComingIntoForceNote;
                parl:treatyComingIntoForceDate ?treatyComingIntoForceDate;
                parl:treatyComingIntoForceNote ?treatyComingIntoForceNote;
                parl:statutoryInstrumentPaperMadeDate ?statutoryInstrumentPaperMadeDate;
                parl:workPackagedThingHasWorkPackagedThingWebLink ?workPackagedThingHasWorkPackagedThingWebLink;
                parl:treatyHasLeadGovernmentOrganisation ?treatyHasLeadGovernmentOrganisation;
                parl:treatyHasCountrySeriesMembership ?treatyHasCountrySeriesMembership;
                parl:treatyHasEuropeanUnionSeriesMembership ?treatyHasEuropeanUnionSeriesMembership;
                parl:treatyHasMiscellaneousSeriesMembership ?treatyHasMiscellaneousSeriesMembership;
                parl:inForceTreatyHasTreatySeriesMembership ?inForceTreatyHasTreatySeriesMembership.
            ?workPackagedThingHasWorkPackagedThingWebLink a parl:WorkPackagedThingWebLink.
            ?treatyHasCountrySeriesMembership a parl:SeriesMembership;
                parl:countrySeriesItemCitation ?countrySeriesItemCitation.
            ?treatyHasEuropeanUnionSeriesMembership a parl:SeriesMembership;
                parl:europeanUnionSeriesItemCitation ?europeanUnionSeriesItemCitation.
            ?treatyHasMiscellaneousSeriesMembership a parl:SeriesMembership;
                parl:miscellaneousSeriesItemCitation ?miscellaneousSeriesItemCitation.
            ?inForceTreatyHasTreatySeriesMembership a parl:SeriesMembership;
                parl:treatySeriesItemCitation ?treatySeriesItemCitation.            
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
            optional {
                ?workPackagedThing parl:treatyName ?workPackagedThingName.
                bind(parl:treatyName as ?workPackagedThingType)
            }
            optional {?workPackagedThing parl:statutoryInstrumentPaperNumber ?statutoryInstrumentPaperNumber}
            optional {?workPackagedThing parl:statutoryInstrumentPaperPrefix ?statutoryInstrumentPaperPrefix}
            optional {?workPackagedThing parl:treatyCommandPaperNumber ?treatyCommandPaperNumber}
            optional {?workPackagedThing parl:treatyCommandPaperPrefix ?treatyCommandPaperPrefix}            
            optional {?workPackagedThing parl:statutoryInstrumentPaperYear ?statutoryInstrumentPaperYear}
            optional {?workPackagedThing parl:statutoryInstrumentPaperComingIntoForceDate ?statutoryInstrumentPaperComingIntoForceDate}
            optional {?workPackagedThing parl:statutoryInstrumentPaperComingIntoForceNote ?statutoryInstrumentPaperComingIntoForceNote}
            optional {?workPackagedThing parl:treatyComingIntoForceDate ?treatyComingIntoForceDate}
            optional {?workPackagedThing parl:treatyComingIntoForceNote ?treatyComingIntoForceNote}
            optional {?workPackagedThing parl:statutoryInstrumentPaperMadeDate ?statutoryInstrumentPaperMadeDate}
            optional {?workPackagedThing parl:workPackagedThingHasWorkPackagedThingWebLink ?workPackagedThingHasWorkPackagedThingWebLink}
            optional {?workPackagedThing parl:treatyHasLeadGovernmentOrganisation ?treatyHasLeadGovernmentOrganisation}
            optional {
                ?workPackagedThing parl:treatyHasCountrySeriesMembership ?treatyHasCountrySeriesMembership.
                optional {?treatyHasCountrySeriesMembership parl:countrySeriesItemCitation ?countrySeriesItemCitation}
            }
            optional {
                ?workPackagedThing parl:treatyHasEuropeanUnionSeriesMembership ?treatyHasEuropeanUnionSeriesMembership.
                optional {?treatyHasEuropeanUnionSeriesMembership parl:europeanUnionSeriesItemCitation ?europeanUnionSeriesItemCitation}
            }
            optional {
                ?workPackagedThing parl:treatyHasMiscellaneousSeriesMembership ?treatyHasMiscellaneousSeriesMembership.
                optional {?treatyHasMiscellaneousSeriesMembership parl:miscellaneousSeriesItemCitation ?miscellaneousSeriesItemCitation}
            }
            optional {
                ?workPackagedThing parl:inForceTreatyHasTreatySeriesMembership ?inForceTreatyHasTreatySeriesMembership.
                optional {?inForceTreatyHasTreatySeriesMembership parl:treatySeriesItemCitation ?treatySeriesItemCitation}
            }            
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select wp.TripleStoreId,
                    coalesce(si.ProcedureStatutoryInstrumentName, nsi.ProcedureProposedNegativeStatutoryInstrumentName, t.ProcedureTreatyName) as WorkPackagedThingName,
	                coalesce(si.StatutoryInstrumentNumber, t.TreatyNumber) as Number,
                    coalesce(si.StatutoryInstrumentNumberPrefix, t.TreatyPrefix) as Prefix,
                    si.StatutoryInstrumentNumberYear, t.LeadGovernmentOrganisationTripleStoreId,
                    coalesce(si.ComingIntoForceNote, t.ComingIntoForceNote) as ComingIntoForceNote,
                    coalesce(si.ComingIntoForceDate, t.ComingIntoForceDate) as ComingIntoForceDate,
                    si.MadeDate, wp.WebLink, cast (0 as bit) as IsDeleted,
                    case
	                    when si.ProcedureStatutoryInstrumentName is not null then 1
                        when nsi.ProcedureProposedNegativeStatutoryInstrumentName is not null then 2
                        when t.ProcedureTreatyName is not null then 3
                    end as WorkPackagedType
                    from ProcedureWorkPackagedThing wp
                    left join ProcedureStatutoryInstrument si on si.Id=wp.Id
                    left join ProcedureProposedNegativeStatutoryInstrument nsi on nsi.Id=wp.Id
                    left join ProcedureTreaty t on t.Id=wp.Id
                    where wp.Id={dataUrl}
                    union
				    select wp.TripleStoreId,
                        null as WorkPackagedThingName,
	                    null as Number,
                        null as Prefix,
                        null as StatutoryInstrumentNumberYear, null as LeadGovernmentOrganisationTripleStoreId,
                        null as ComingIntoForceNote, null as ComingIntoForceDate,
                        null as MadeDate, null as WebLink, cast (1 as bit) as IsDeleted,
	                    null as WorkPackagedType
                    from DeletedProcedureWorkPackagedThing wp
                    where wp.Id={dataUrl};
                    select t.Id, sm.TripleStoreId, sm.Citation, 
	                    case when cs.Id is null then 
		                    case when eus.Id is null then 
			                    case when ms.Id is null then null
	                    else 3 end else 2 end else 1 end as SeriesMembershipId	
                    from ProcedureTreaty t
                    left join ProcerdureCountrySeriesMembership cs on cs.ProcedureTreatyId=t.Id
                    left join ProcerdureEuropeanUnionSeriesMembership eus on eus.ProcedureTreatyId=t.Id
                    left join ProcerdureMiscellaneousSeriesMembership ms on ms.ProcedureTreatyId=t.Id
                    left join ProcedureSeriesMembership sm on sm.Id=coalesce(cs.Id,eus.Id,ms.Id)
                    where t.Id={dataUrl}
                    union
                    select t.Id, sm.TripleStoreId, sm.Citation, 4 as SeriesMembershipId
                    from ProcedureTreaty t
                    join ProcerdureTreatySeriesMembership ts on ts.ProcedureTreatyId=t.Id
                    join ProcedureSeriesMembership sm on sm.Id=ts.Id
                    where t.Id={dataUrl}";
        }
    }
}
