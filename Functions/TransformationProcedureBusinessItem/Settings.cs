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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "d4c67a7c-6254-4b18-a936-4155536811e5").Replace("{id}", dataUrl);
        }
    }
}
