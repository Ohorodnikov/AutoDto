using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;
using AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

namespace AutoDto.SourceGen.MetadataUpdaters;

internal interface IMetadataUpdaterHelper
{
    void InitByAttributes(IDtoTypeMetadata metadata, IEnumerable<IDtoAttributeData> attributes);
    void InitByAttribute(IDtoTypeMetadata metadata, IDtoAttributeData attributeData);
    void ApplyCommonRules(Dictionary<string, List<IDtoTypeMetadata>> metadatas);
}

internal class MetadataUpdaterHelper : IMetadataUpdaterHelper
{
    private readonly IAttributeUpdaterFactory _attributeUpdaterFactory;
    private readonly IEnumerable<IAllMetadatasUpdater> _allMetadataUpdaters;

    public MetadataUpdaterHelper(IAttributeUpdaterFactory attributeUpdaterFactory, IEnumerable<IAllMetadatasUpdater> allMetadataUpdaters)
    {
        _attributeUpdaterFactory = attributeUpdaterFactory;
        _allMetadataUpdaters = allMetadataUpdaters;
    }

    public void InitByAttributes(IDtoTypeMetadata metadata, IEnumerable<IDtoAttributeData> attributes)
    {
        var dtoFrom = attributes.OfType<DtoFromData>().Single();

        InitByAttribute(metadata, dtoFrom); //should be first as main attribute

        foreach (var attr in attributes)
        {
            if (attr == dtoFrom)
                continue;

            InitByAttribute(metadata, attr);
        }
    }

    public void InitByAttribute(IDtoTypeMetadata metadata, IDtoAttributeData attributeData)
    {
        _attributeUpdaterFactory
            .GetUpdater(attributeData)
            .Update(metadata, attributeData);
    }

    public void ApplyCommonRules(Dictionary<string, List<IDtoTypeMetadata>> metadatas)
    {
        foreach (var updater in _allMetadataUpdaters)
            updater.UpdateAllMetadata(metadatas);
    }
}