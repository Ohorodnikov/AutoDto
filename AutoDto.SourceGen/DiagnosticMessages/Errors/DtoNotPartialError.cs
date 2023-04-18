namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal class DtoNotPartialError : ErrorDiagnosticMessage
{
    public DtoNotPartialError(string dtoTypeName) 
        : base(
            "7", 
            "Dto must be marked as partial", 
            "Dto {0} must be partial",
            new[] {dtoTypeName} )
    {
    }
}
