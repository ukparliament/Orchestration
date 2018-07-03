using System;

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
                ?workPackageHasWorkPackageableThing ?workPackageableThing.
            ?workPackageHasProcedure a parl:Procedure.
            ?workPackageableThing a parl:WorkPackageableThing.
        }
        where {
            bind(@subject as ?workPackage)
            ?workPackage parl:workPackageHasProcedure ?workPackageHasProcedure.
            optional {
                ?workPackage parl:workPackageHasStatutoryInstrument ?workPackageableThing.
                bind(parl:workPackageHasStatutoryInstrument as ?workPackageHasWorkPackageableThing)                    
            }
            optional {
                ?workPackage parl:workPackageHasProposedStatutoryInstrument ?workPackageableThing.
                bind(parl:workPackageHasProposedStatutoryInstrument as ?workPackageHasWorkPackageableThing)
            }            
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select wp.ProcedureWorkPackageTripleStoreId as TripleStoreId, p.TripleStoreId as [Procedure], 
                    wp.TripleStoreId as WorkPackageableThing, 
                    t.ProcedureWorkPackageableThingTypeName, wp.IsDeleted from ProcedureWorkPackageableThing wp
                join [Procedure] p on p.Id=wp.ProcedureId
                join ProcedureWorkPackageableThingType t on t.Id=wp.ProcedureWorkPackageableThingTypeId
                where wp.Id={dataUrl}";
        }
    }
}
