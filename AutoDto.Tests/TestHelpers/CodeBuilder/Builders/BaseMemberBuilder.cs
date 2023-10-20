namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public abstract class BaseMemberBuilder<TMember> : BaseElementBuilder<TMember>
{
    protected BaseMemberBuilder(string name, Type returnType) 
        : this(name, returnType.Name)
    {
    }

    protected BaseMemberBuilder(string name, string returnType) 
        : base(name)
    {
        ReturnType = returnType;
    }

    protected string ReturnType { get; set; }
    public BaseMemberBuilder<TMember> SetReturnType(Type returnType)
    {
        ReturnType = returnType.Name;
        return this;
    }

    public BaseMemberBuilder<TMember> SetReturnType(string returnType)
    {
        ReturnType = returnType;
        return this;
    }
}

