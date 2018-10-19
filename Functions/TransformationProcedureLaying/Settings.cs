namespace Functions.TransformationProcedureLaying
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
            ?laying a parl:Laying;
                parl:layingHasLaidThing ?layingHasLaidThing;
                parl:layingHasLayingBody ?layingHasLayingBody;
                parl:layingDate ?layingDate.
            ?layingHasLayingBody a parl:LayingBody.
            ?layingHasLaidThing a parl:LaidThing.
        }
        where {
            bind(@subject as ?laying)
            ?laying parl:layingHasLaidThing ?layingHasLaidThing.
            optional {?laying parl:layingHasLayingBody ?layingHasLayingBody}
            optional {?laying parl:layingDate ?layingDate}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select b.TripleStoreId, lb.TripleStoreId as LayingBody, 
	                wp.TripleStoreId as WorkPackaged, li.LayingDate, cast(0 as bit) as IsDeleted
                from ProcedureLaying li
                left join LayingBody lb on lb.Id=li.LayingBodyId
                join ProcedureBusinessItem b on b.Id=li.ProcedureBusinessItemId
                join ProcedureWorkPackagedThing wp on wp.Id=li.ProcedureWorkPackagedId
                where li.Id={dataUrl}
                union
                select li.TripleStoreId, null as LayingBody, 
	                null as WorkPackaged, null as LayingDate, cast(0 as bit) as IsDeleted
                from DeletedProcedureLaying li
                where li.Id={dataUrl}";
        }
    }
}
