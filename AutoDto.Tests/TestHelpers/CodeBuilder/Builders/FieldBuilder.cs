using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class FieldBuilder : BaseMemberBuilder<FieldMember>
{
    public FieldBuilder(string name) : base(name)
    {
    }

    protected override FieldMember BuildImpl()
    {
        return new FieldMember(GetBaseDeclaration(), ReturnType);
    }

    protected override void ValidateSetup()
    {
        base.ValidateSetup();

        if (ReturnType == "void")
            throw new InvalidOperationException("Return type 'void' is not allowed");
    }
}

