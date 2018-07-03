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
        	    parl:procedureName ?procedureName.
        }
        where {
            bind(@subject as ?procedure)
            ?procedure parl:procedureName ?procedureName.
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select p.TripleStoreId, p.ProcedureName, p.IsDeleted from [Procedure] p 
            where p.Id={dataUrl}";
        }
    }
}
