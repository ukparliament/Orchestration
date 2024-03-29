﻿namespace Functions.TransformationProcedureBusinessItem
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
            ?businessItem a parl:BusinessItem;
                parl:businessItemHasWorkPackage ?businessItemHasWorkPackage;
                parl:businessItemHasProcedureStep ?businessItemHasProcedureStep;
                parl:businessItemDate ?businessItemDate;
                parl:businessItemHasBusinessItemWebLink ?businessItemHasBusinessItemWebLink.
            ?businessItemHasWorkPackage a parl:WorkPackage.
            ?businessItemHasProcedureStep a parl:ProcedureStep.
            ?businessItemHasBusinessItemWebLink a parl:BusinessItemWebLink.
        }
        where {
            bind(@subject as ?businessItem)
            ?businessItem parl:businessItemHasWorkPackage ?businessItemHasWorkPackage.
            optional {?businessItem parl:businessItemHasProcedureStep ?businessItemHasProcedureStep}
            optional {?businessItem parl:businessItemDate ?businessItemDate}
            optional {?businessItem parl:businessItemHasBusinessItemWebLink ?businessItemHasBusinessItemWebLink}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select bi.TripleStoreId, bi.WebLink,
	                bi.BusinessItemDate, 
                    wp.ProcedureWorkPackageTripleStoreId as WorkPackage,
	                cast(0 as bit) as IsDeleted
                from ProcedureBusinessItem bi
                join ProcedureWorkPackagedThing wp on wp.Id=bi.ProcedureWorkPackageId
                where bi.Id={dataUrl}
                union
				select bi.TripleStoreId, null as WebLink,
	                null BusinessItemDate,
                    null as WorkPackage,
	                cast(1 as bit) as IsDeleted
				from DeletedProcedureBusinessItem bi
				where bi.Id={dataUrl};
                select s.TripleStoreId from ProcedureBusinessItemProcedureStep bi
                join ProcedureStep s on s.Id=bi.ProcedureStepId
                where bi.ProcedureBusinessItemId={dataUrl}";
        }
    }
}
