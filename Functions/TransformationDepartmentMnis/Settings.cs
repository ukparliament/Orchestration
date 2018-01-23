namespace Functions.TransformationDepartmentMnis
{
    public class Settings : ITransformationSettings
    {

        public string AcceptHeader
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public string SubjectRetrievalSparqlCommand
        {
            get
            {
                return @"
            construct{
                ?s a parl:Group.
            }
            where{
                ?s parl:mnisDepartmentId @mnisDepartmentId.
            }";
            }
        }

        public string ExistingGraphSparqlCommand
        {
            get
            {
                return @"
        construct {
        	?department a parl:Group;
                parl:mnisDepartmentId ?mnisDepartmentId;
                parl:groupName ?groupName;
                parl:groupStartDate ?groupStartDate;
                parl:groupEndDate ?groupEndDate.        
        }
        where {
            bind(@subject as ?department)
        	?department parl:mnisDepartmentId ?mnisDepartmentId.
            optional {?department parl:groupName ?groupName}
            optional {?department parl:groupStartDate ?groupStartDate}
            optional {?department parl:groupEndDate ?groupEndDate}            
        }";
            }
        }

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return $"{dataUrl}?$select=Department_Id,Name,StartDate,EndDate";
        }
    }
}
