namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal class MoreThanOneDtoDeclarationAttributeError : ErrorDiagnosticMessage
{
    public MoreThanOneDtoDeclarationAttributeError()
        : base (
            "8", 
            "More than one dto declaration attr",
            "Only one attribute is allowed to mark a dto class"
            )
    {
    }
}
