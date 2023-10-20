using AutoDto.Setup;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class DtoClassBuilder : ClassBuilder
{
    public enum DtoAttributeType
    {
        NotInited,
        DtoFrom,
        //DtoFor
    }

    public DtoClassBuilder(string name, DtoAttributeType dtoAttributeType, Type blType)
        : this(name, dtoAttributeType, blType.Name, blType.Namespace)
    {
    }

    public DtoClassBuilder(string name, DtoAttributeType dtoAttributeType, string blType, string blTypeNamespace = null)
        : base(name)
    {
        DtoAttribute = dtoAttributeType;
        BlType = blType;
        AddUsing(blTypeNamespace);
        SetPartial(true);
    }

    protected string BlType { get; private set; }
    protected DtoAttributeType DtoAttribute { get; private set; } = DtoAttributeType.NotInited;
    protected RelationStrategy Strategy { get; private set; } = RelationStrategy.None;    

    protected override ClassElement BuildImpl()
    {
        AddAttribute(CreateDtoAttribute());
        AddUsing(typeof(DtoFromAttribute).Namespace);

        return base.BuildImpl();
    }

    protected override void ValidateSetup()
    {
        base.ValidateSetup();

        if (string.IsNullOrEmpty(BlType))
            throw new ArgumentException("Param is mandatory", nameof(BlType));

        if (DtoAttribute == DtoAttributeType.NotInited)
            throw new ArgumentOutOfRangeException("Param is mandatory", nameof(DtoAttribute));
    }

    private AttributeInfo CreateDtoAttribute()
    {
        var name = nameof(DtoFromAttribute);

        name = name.Substring(0, name.Length - nameof(Attribute).Length);

        return new AttributeInfo(name, new[] { $"typeof({BlType})", $"RelationStrategy.{Strategy}" });
    }

    public DtoClassBuilder SetRelationStrategy(RelationStrategy relationStrategy)
    {
        Strategy = relationStrategy;
        return this;
    }
}

