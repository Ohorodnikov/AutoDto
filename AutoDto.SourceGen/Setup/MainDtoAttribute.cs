namespace AutoDto.Setup;

[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DtoMainAttribute : Attribute
{
    public DtoMainAttribute()
    {
    }
}
