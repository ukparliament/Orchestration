using System;

namespace Functions.TransformationProcedure
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
            ?procedure a parl:Procedure;
        	    parl:procedureName ?procedureName;
                parl:procedureDescription ?procedureDescription.
        }
        where {
            bind(@subject as ?procedure)
            ?procedure parl:procedureName ?procedureName.
            optional {?procedure parl:procedureDescription ?procedureDescription}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select p.TripleStoreId, p.ProcedureName, p.ProcedureDescription, cast(0 as bit) as IsDeleted from [Procedure] p 
                where p.Id={dataUrl}
                union
                select p.TripleStoreId, null as ProcedureName, null ProcedureDescription, cast(1 as bit) as IsDeleted from [DeletedProcedure] p 
                where p.Id={dataUrl}";
        }
    }
}
