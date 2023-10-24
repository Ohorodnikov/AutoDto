using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class PropertyBuilder : BaseMemberBuilder<PropertyMember>
{
    protected class AccessorInfo
    {
        public AccessorInfo(Visibility visibility, bool isExists)
        {
            Visibility = visibility;
            IsExists = isExists;
        }
        public Visibility Visibility { get; set; }
        public bool IsExists { get; set; }
    }

    public PropertyBuilder(string name, Type returnType) 
        : this(name, returnType.Name) 
    { 
    }

    public PropertyBuilder(string name, string returnType) 
        : base(name, returnType)
    {
        Get = new(Visibility, true);
        Set = new(Visibility, true);
    }

    protected AccessorInfo Get { get; }
    protected AccessorInfo Set { get; }

    protected override PropertyMember BuildImpl()
    {
        return new PropertyMember(GetBaseDeclaration(), ReturnType, (Get.Visibility, Get.IsExists), (Set.Visibility, Set.IsExists));
    }

    public override BaseElementBuilder<PropertyMember> SetAccessor(Visibility visibility)
    {
        Get.Visibility = visibility;
        Set.Visibility = visibility;
        base.SetAccessor(visibility);
        return this;
    }

    public PropertyBuilder DefineGet(bool isExists)
    {
        Get.IsExists = isExists;
        return this;
    }

    public PropertyBuilder DefineGet(Visibility visibility)
    {
        Get.Visibility = visibility;
        return this;
    }

    public PropertyBuilder DefineSet(bool isExists)
    {
        Set.IsExists = isExists;
        return this;
    }

    public PropertyBuilder DefineSet(Visibility visibility)
    {
        Set.Visibility = visibility;
        return this;
    }

    protected override void ValidateSetup()
    {
        base.ValidateSetup();

        if (ReturnType == "void")
            throw new InvalidOperationException("Return type 'void' is not allowed");
    }
}

