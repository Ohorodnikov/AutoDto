namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public class ConstantMember : Member
{
    public ConstantMember(string fullCode) : base(fullCode)
    {
    }

    public ConstantMember(BaseDeclarationInfo declarationInfo, string returnType, string value) 
        : base(declarationInfo, returnType)
    {
        Value = value;
    }

    public string Value { get; }

    protected override string GenerateCodeFromDeclaration()
    {
        return $"{GetVisibilityString()} const {ReturnType} {Name} = {Value};";
    }
}

