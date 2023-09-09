using AutoDto.Setup;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal interface IAttributeDataFactory
{
    IDtoAttributeData Get(string attributeFullName);
    IDtoAttributeData Get(Attribute attribute);
    IDtoAttributeData Get(Type attributeType);
    IDtoAttributeData Get(AttributeData attributeData);
}

internal class AttributeDataFactory : IAttributeDataFactory
{
    private static readonly Dictionary<string, Func<IDtoAttributeData>> _attr2Data = new Dictionary<string, Func<IDtoAttributeData>>
    {
        { typeof(DtoFromAttribute).FullName, () => new DtoFromData() },
        { typeof(DtoForAttribute).FullName, () => new DtoForData() },
        { typeof(DtoIgnoreAttribute).FullName, () => new DtoIgnoreData() },
        { typeof(DtoMainAttribute).FullName, () => new DtoMainData() },
    };

    public IDtoAttributeData Get(string attributeFullName)
    {
        return _attr2Data.TryGetValue(attributeFullName, out var attr) ? attr() : null;
    }

    public IDtoAttributeData Get(Attribute attribute) => Get(attribute.GetType());
    public IDtoAttributeData Get(Type attributeType) => Get(attributeType.FullName);
    public IDtoAttributeData Get(AttributeData attributeData) => Get(attributeData.AttributeClass.ToDisplayString());
}
