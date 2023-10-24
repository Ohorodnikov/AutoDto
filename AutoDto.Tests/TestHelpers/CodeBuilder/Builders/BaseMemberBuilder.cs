using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public abstract class BaseMemberBuilder<TMember> : BaseElementBuilder<TMember>
    where TMember : Member
{
    protected BaseMemberBuilder(string name, Type returnType) 
        : this(name, returnType == typeof(void) ? "void" : returnType.Name)
    {
    }

    protected BaseMemberBuilder(string name, string returnType) 
        : base(name)
    {
        ReturnType = returnType;
    }

    protected string ReturnType { get; set; }
    public virtual BaseMemberBuilder<TMember> SetReturnType(Type returnType)
    {
        return SetReturnType(returnType.Name);
    }

    public virtual BaseMemberBuilder<TMember> SetReturnType(string returnType)
    {
        ReturnType = returnType;
        return this;
    }
}

