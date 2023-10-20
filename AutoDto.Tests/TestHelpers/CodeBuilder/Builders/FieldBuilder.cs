using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class FieldBuilder : BaseMemberBuilder<FieldMember>
{
    public FieldBuilder(string name, Type returnType) 
        : base(name, returnType) 
    { 
    }

    public FieldBuilder(string name, string returnType) 
        : base(name, returnType)
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

