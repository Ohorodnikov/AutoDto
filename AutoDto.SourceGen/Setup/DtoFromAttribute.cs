namespace AutoDto.Setup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DtoFromAttribute : Attribute
{
    public Type Type { get; }

    public DtoFromAttribute(Type type, RelationStrategy relationStrategy = RelationStrategy.None)
    {
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DtoForAttribute : Attribute
{
    public Type Type { get; }

    public DtoForAttribute(Type type, RelationStrategy relationStrategy = RelationStrategy.None)
    {
        Type = type;
    }
}