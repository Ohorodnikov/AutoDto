using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class PropertyBuilder : BaseMemberBuilder<PropertyMember>
{
    public PropertyBuilder(string name) : base(name)
    {
        Get = (Visibility, true);
        Set = (Visibility, true);
    }

    protected (Visibility visibitity, bool isExists) Get { get; set; }
    protected (Visibility visibitity, bool isExists) Set { get; set; }

    protected override PropertyMember BuildImpl()
    {
        return new PropertyMember(GetBaseDeclaration(), ReturnType, Get, Set);
    }

    public PropertyBuilder DefineGet(bool isExists, Visibility visibility)
    {
        Get = (visibility, isExists);
        return this;
    }

    public PropertyBuilder DefineSet(bool isExists, Visibility visibility)
    {
        Set = (visibility, isExists);
        return this;
    }

    protected override void ValidateSetup()
    {
        base.ValidateSetup();

        if (ReturnType == "void")
            throw new InvalidOperationException("Return type 'void' is not allowed");
    }
}

