using System.Runtime.Serialization;

namespace Functions.TransformationProcedureWorkPackage
{
    public enum ProcedureWorkPackageableThingType
    {
        [EnumMember(Value = "Statutory Instrument")]
        StatutoryInstrument,

        [EnumMember(Value = "Proposed Statutory Instrument")]
        ProposedStatutoryInstrument
    }
}
