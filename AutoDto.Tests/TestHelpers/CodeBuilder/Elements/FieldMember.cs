namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public class FieldMember : Member
{
    public FieldMember(string fullCode) : base(fullCode) { }

    public FieldMember(BaseDeclarationInfo declarationInfo, string returnType)
        : base(declarationInfo, returnType)
    {
    }

    protected override string GenerateCodeFromDeclaration() => GetDefinitionString() + ";";
}

