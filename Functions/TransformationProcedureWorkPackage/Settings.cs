namespace Functions.TransformationProcedureWorkPackage
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
            ?workPackage a parl:WorkPackage;
                parl:workPackageHasProcedure ?workPackageHasProcedure;
                parl:workPackageHasWorkPackagedThing ?workPackageHasWorkPackagedThing.
            ?workPackageHasProcedure a parl:Procedure.
            ?workPackageHasWorkPackagedThing a parl:WorkPackagedThing.
        }
        where {
            bind(@subject as ?workPackage)
            ?workPackage parl:workPackageHasProcedure ?workPackageHasProcedure.
            optional {?workPackage parl:workPackageHasWorkPackagedThing ?workPackageHasWorkPackagedThing}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select wp.ProcedureWorkPackageTripleStoreId as TripleStoreId,
	                wp.TripleStoreId as WorkPackagedThing, p.TripleStoreId as [Procedure],
	                cast(0 as bit) as IsDeleted
                from ProcedureWorkPackagedThing wp
                join [Procedure] p on p.Id=wp.ProcedureId
                where wp.Id={dataUrl}
                union
				select wp.ProcedureWorkPackageTripleStoreId as TripleStoreId,
	                null as WorkPackagedThing, null as [Procedure],
	                cast(1 as bit) as IsDeleted
                from DeletedProcedureWorkPackagedThing wp
                where wp.Id={dataUrl}";
        }
    }
}
