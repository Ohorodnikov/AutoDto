namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public class MethodMember : Member
{
    public MethodMember(string fullCode) : base(fullCode) { }

    public MethodMember(BaseDeclarationInfo declarationInfo, string returnType, List<(string, string)> arguments, string body)
        : base(declarationInfo, returnType)
    {
        Arguments = arguments;
        Body = body;
    }

    public List<(string, string)> Arguments { get; }
    public string Body { get; }

    protected override string GenerateCodeFromDeclaration()
    {
        var args = Arguments.Select(x => $"{x.Item1} {x.Item2}").ToList();
        return
$@"{GetDefinitionString()}({string.Join(", ", args)})
{{
{Body}
}}";
    }
}

