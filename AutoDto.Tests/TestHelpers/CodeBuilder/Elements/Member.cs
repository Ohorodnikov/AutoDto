namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public abstract class Member : CodeElement
{
    protected Member(string fullCode) : base(fullCode) { }

    protected Member(BaseDeclarationInfo declarationInfo, string returnType) : base(declarationInfo)
    {
        ReturnType = returnType;
    }

    public string ReturnType { get; }

    protected string GetDefinitionString()
    {
        return $"{GetAttributesString()} {GetVisibilityString()} {GetStaticString()} {GetPartialString()} {GetInheritanceString()} {ReturnType} {Name}";
    }
}

