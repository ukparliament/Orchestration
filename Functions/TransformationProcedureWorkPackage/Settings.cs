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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "25101afd-6e12-4e13-b981-e3b2a12f112e").Replace("{id}", dataUrl);
        }
    }
}
