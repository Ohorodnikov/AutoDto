namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal class AttributeNullError : ErrorDiagnosticMessage
{
    public AttributeNullError() : base("1", "Null argument", "Null value is invalid in this context")
    {
    }
}

internal class AttributeValueNotSetError : ErrorDiagnosticMessage
{
    public AttributeValueNotSetError() : base("2", "Parameters not found", "Attribute parameters must be filled")
    {
    }
}
