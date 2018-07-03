using System;

namespace Functions.TransformationProcedureStep
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
            ?procedureStep a parl:ProcedureStep;
        	    parl:procedureStepName ?procedureStepName;
                parl:procedureStepDescription ?procedureStepDescription;
                parl:procedureStepHasHouse ?procedureStepHasHouse.
            ?procedureStepHasHouse a parl:House.
        }
        where {
            bind(@subject as ?procedureStep)
            ?procedureStep parl:procedureStepName ?procedureStepName.
            optional {?procedureStep parl:procedureStepDescription ?procedureStepDescription}
            optional {?procedureStep parl:procedureStepHasHouse ?procedureStepHasHouse}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select s.TripleStoreId, s.ProcedureStepName, 
                    s.ProcedureStepDescription, s.IsDeleted from ProcedureStep s
                where s.Id={dataUrl};
                select h.TripleStoreId as House from ProcedureStepHouse s
                join House h on h.Id=s.HouseId
                where s.ProcedureStepId={dataUrl}";
        }
    }
}
