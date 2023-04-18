using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Helpers;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

internal interface IAttributeUpdaterFactory
{
    IAttributeMetadataUpdater GetUpdater(Type attributeDataType);
    IAttributeMetadataUpdater GetUpdater(IDtoAttributeData attributeData);
}

internal class AttributeUpdaterFactory : IAttributeUpdaterFactory
{
    private static Dictionary<Type, Func<IAttributeMetadataUpdater>> _metadataMap = new Dictionary<Type, Func<IAttributeMetadataUpdater>>
    {
        { typeof(DtoFromData), () => new DtoFromMetadataUpdater() },
        { typeof(DtoIgnoreData), () => new DtoIgnoreMetadataUpdater() },
        { typeof(DtoMainData), () => new DtoMainMetadataUpdater() },
    };

    private Dictionary<Type, IAttributeMetadataUpdater> _metadataUpdaters = new Dictionary<Type, IAttributeMetadataUpdater>();

    public IAttributeMetadataUpdater GetUpdater(Type attributeDataType)
        => _metadataUpdaters.GetOrAdd(attributeDataType, _metadataMap[attributeDataType]);

    public IAttributeMetadataUpdater GetUpdater(IDtoAttributeData attributeData)
        => GetUpdater(attributeData.GetType());
}
