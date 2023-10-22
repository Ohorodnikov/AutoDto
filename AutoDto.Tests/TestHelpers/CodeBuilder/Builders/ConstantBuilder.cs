using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class ConstantBuilder : BaseMemberBuilder<ConstantMember>
{
    public ConstantBuilder(string name, Type returnType, string value) 
        : base(name, returnType)
    {
        Value = value;
    }

    protected string Value { get; }

    protected override ConstantMember BuildImpl()
    {
        return new ConstantMember(GetBaseDeclaration(), ReturnType, Value);
    }
}

