using System;

namespace Functions.TransformationProcedureBusinessItem
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
                parl:businessItemHasBusinessItemWebLink ?businessItemHasBusinessItemWebLink;
                parl:layingHasLayingBody ?layingHasLayingBody;
                parl:layingHasLayableThing ?layingHasLayableThing.
            ?businessItemHasWorkPackage a parl:WorkPackage.
            ?businessItemHasProcedureStep a parl:ProcedureStep.
            ?businessItemHasBusinessItemWebLink a parl:BusinessItemWebLink.
            ?layingHasLayingBody a parl:LayingBody.
            ?layingHasLayableThing a parl:LayableThing.
        }
        where {
            bind(@subject as ?businessItem)
            ?businessItem parl:businessItemHasWorkPackage ?businessItemHasWorkPackage.
            optional {?businessItem parl:businessItemHasProcedureStep ?businessItemHasProcedureStep}
            optional {?businessItem parl:businessItemDate ?businessItemDate}
            optional {?businessItem parl:businessItemHasBusinessItemWebLink ?businessItemHasBusinessItemWebLink}
            optional {?businessItem parl:layingHasLayingBody ?layingHasLayingBody}
            optional {?businessItem parl:layingHasLayableThing ?layingHasLayableThing}
        }";
            }
        }

        public string ParameterizedString(string dataUrl)
        {
            return $@"select bi.TripleStoreId, bi.WebLink, ab.TripleStoreId as AnsweringBody,
                bi.BusinessItemDate, bi.IsDeleted from ProcedureBusinessItem bi 
            left join LayingBody ab on ab.Id=bi.LayingBodyId
            where bi.Id={dataUrl};
            select wpt.ProcedureWorkPackageTripleStoreId as WorkPackage, wpt.TripleStoreId as WorkPackageableThing from ProcedureBusinessItem bi 
            join ProcedureWorkPackageableThing wpt on wpt.Id=bi.ProcedureWorkPackageId
            where bi.Id={dataUrl};
            select s.TripleStoreId as Step from ProcedureBusinessItemProcedureStep bi 
            join ProcedureStep s on s.Id=bi.ProcedureStepId
            where bi.ProcedureBusinessItemId={dataUrl}";
        }
    }
}
