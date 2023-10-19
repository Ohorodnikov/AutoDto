using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class MethodBuilder : BaseMemberBuilder<MethodMember>
{
    public MethodBuilder(string name) : base(name)
    {
    }

    protected string Body { get; private set; }
    protected List<(string, string)> Arguments { get; private set; } = new List<(string, string)>();

    protected override MethodMember BuildImpl()
    {
        return new MethodMember(GetBaseDeclaration(), ReturnType, Arguments, Body);
    }

    public MethodBuilder SetBody(string body)
    {
        Body = body;
        return this;
    }

    public MethodBuilder AddArgument(string type, string name)
    {
        Arguments.Add((type, name));
        return this;
    }
}

