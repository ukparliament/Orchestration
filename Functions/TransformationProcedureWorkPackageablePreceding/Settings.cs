using System;

namespace Functions.TransformationProcedureWorkPackageablePreceding
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
                parl:workPackageableThingPrecedesWorkPackageableThing ?workPackageableThingPrecedesWorkPackageableThing.
            ?workPackageableThingPrecedesWorkPackageableThing a parl:WorkPackageableThing.
        }
        where {
            bind(@subject as ?workPackageableThing)
            ?workPackageableThing parl:workPackageableThingPrecedesWorkPackageableThing ?workPackageableThingPrecedesWorkPackageableThing.
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select pwpt.TripleStoreId as PrecedingWorkPackageable, fwpt.TripleStoreId as FollowingWorkPackageable, wptp.IsDeleted from ProcedureWorkPackageableThingPreceding wptp
                join ProcedureWorkPackageableThing pwpt on pwpt.Id=wptp.PrecedingProcedureWorkPackageableThingId
                join ProcedureWorkPackageableThing fwpt on fwpt.Id=wptp.FollowingProcedureWorkPackageableThingId
                where wptp.Id={dataUrl}";
        }
    }
}
