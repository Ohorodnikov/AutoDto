namespace AutoDto.SourceGen.DiagnosticMessages.Warnings;

internal class NotFoundPropertyInBlWarn : WarningDiagnosticMessage
{
    public NotFoundPropertyInBlWarn(string propName, string blClassName) 
        : base("1", "Invalid property", "Cannot find property '{0}' in parent class '{1}'", new[] {propName, blClassName})
    {
    }
}

internal class PropertyConflictWarning : WarningDiagnosticMessage
{
    public PropertyConflictWarning(string propName, string dtoName) 
        : base(
            "2",
            "Property name conflict",
            "DTO {0} already has property {1}",
            new[] {dtoName, propName}
            )
    {
    }
}

internal class MemberConflictWarning : WarningDiagnosticMessage
{
    public MemberConflictWarning(string propName, string dtoName)
       : base(
           "3",
           "Member name conflict",
           "DTO {0} already has member {1}",
           new[] { dtoName, propName }
           )
    {
    }
}
