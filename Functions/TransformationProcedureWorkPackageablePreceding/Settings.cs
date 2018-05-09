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

        public string FullDataUrlParameterizedString(string dataUrl)
        {
            return System.Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SharepointItem", EnvironmentVariableTarget.Process).Replace("{listId}", "24a1a168-c956-4da5-a7e7-aaf8ed54b38c").Replace("{id}", dataUrl);
        }
    }
}
