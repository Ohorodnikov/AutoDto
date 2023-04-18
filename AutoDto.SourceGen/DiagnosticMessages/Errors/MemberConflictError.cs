namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal class MemberConflictError : ErrorDiagnosticMessage
{
    public MemberConflictError(string memberName, string dtoName) 
        : base(
            "5", 
            "Member name conflict", 
            "Cannot generate DTO {0} with property {1}. Type {0} already contains member with name {1}. Use [DtoIgnore] to ignore property {1}",
            new[] {dtoName, memberName} )
    {
    }
}

internal class ReservedMemberConflicError : ErrorDiagnosticMessage
{
    public ReservedMemberConflicError(string reservedName, string dtoName) 
        : base(
            "6", 
            "Member has conflict with reserved name", 
            "Cannot generate DTO {0} due to conflict to reserved name {1}.",
            new[] {dtoName, reservedName} )
    {
    }
}
