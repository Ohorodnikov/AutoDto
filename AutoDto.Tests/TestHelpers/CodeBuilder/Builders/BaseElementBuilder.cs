using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public abstract class BaseElementBuilder<TElementInfo>
{
    public BaseElementBuilder(string name)
    {
        Name = name;
    }

    protected string Name { get; set; }
    protected bool IsPartial { get; set; } = false;
    protected bool IsStatic { get; set; } = false;
    protected InheritanceStatus InheritanceStatus { get; set; } = InheritanceStatus.None;
    protected List<AttributeInfo> Attributes { get; set; } = new List<AttributeInfo>();
    protected Visibility Visibility { get; set; } = Visibility.Public;

    public TElementInfo Build()
    {
        ValidateSetup();
        return BuildImpl();
    }

    protected abstract TElementInfo BuildImpl();

    protected virtual void ValidateSetup()
    {
        if (string.IsNullOrEmpty(Name))
            throw new InvalidOperationException("Class name is mandatory");
    }

    protected BaseDeclarationInfo GetBaseDeclaration()
    {
        return new BaseDeclarationInfo(Visibility, IsStatic, IsPartial, InheritanceStatus, Name, Attributes);
    }

    public BaseElementBuilder<TElementInfo> SetAccessor(Visibility visibility)
    {
        Visibility = visibility;
        return this;
    }

    public BaseElementBuilder<TElementInfo> SetPartial(bool isPartial = true)
    {
        IsPartial = isPartial;
        return this;
    }

    public BaseElementBuilder<TElementInfo> SetInheritance(InheritanceStatus inheritanceStatus)
    {
        InheritanceStatus = inheritanceStatus;
        return this;
    }

    public BaseElementBuilder<TElementInfo> SetStatic(bool isStatic = true)
    {
        IsStatic = isStatic;
        return this;
    }

    public BaseElementBuilder<TElementInfo> SetName(string name)
    {
        Name = name;
        return this;
    }

    public BaseElementBuilder<TElementInfo> AddAttribute(Type attribute, params string[] arguments)
    {
        var baseCl = attribute;
        var typeIsAttr = false;
        while (baseCl != typeof(object))
        {
            typeIsAttr = baseCl == typeof(Attribute);
            if (typeIsAttr)
                break;

            baseCl = baseCl.BaseType;
        }

        if (!typeIsAttr)
            throw new ArgumentException("Type is not Attribute", nameof(attribute));

        return AddAttribute(attribute.Name.Replace(nameof(Attribute), ""), attribute.Namespace, arguments);
    }

    public virtual BaseElementBuilder<TElementInfo> AddAttribute(string name, string @namespace = null, params string[] arguments)
    {
        if (@namespace != null)
            throw new NotImplementedException("Do not add namespace here. Add it on class-level building or use full attribute name");

        return AddAttribute(new AttributeInfo(name, arguments));
    }

    public BaseElementBuilder<TElementInfo> AddAttribute(AttributeInfo attributeInfo)
    {
        Attributes.Add(attributeInfo);
        return this;
    }

    public T As<T>()
        where T : BaseElementBuilder<TElementInfo>
    {
        return (T)this;
    }
}

