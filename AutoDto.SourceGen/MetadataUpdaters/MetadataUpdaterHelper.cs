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
        var dtoFrom = attributes.OfType<DtoFromData>().SingleOrDefault();
        var dtoFor = attributes.OfType<DtoForData>().SingleOrDefault();

        if (dtoFrom != null && dtoFor != null)
            throw new NotSupportedException("Only one attribute is allowed");

        if (dtoFrom == null && dtoFor == null)
            throw new NotSupportedException("One of attributes [DtoFrom], [DtoFor] must exists");

        InitByAttribute(metadata, dtoFrom ?? dtoFor); //should be first as main attribute

        foreach (var attr in attributes)
        {
            if (attr == dtoFrom)
                continue;

            InitByAttribute(metadata, attr);
        }
    }

    public void InitByAttribute(IDtoTypeMetadata metadata, IDtoAttributeData attributeData)
    {
        var updater = _attributeUpdaterFactory.GetUpdater(attributeData);

        if (updater == null)
        {
            LogHelper.Logger.Warning("Cannot find attribute updater for attribute data {name}", attributeData.GetType().Name);
            return;
        }

        LogHelper.Logger.Verbose("Apply {updater} for {attrData}", updater.GetType().Name, attributeData.GetType().Name);

        updater.Update(metadata, attributeData);
    }

    public void ApplyCommonRules(Dictionary<string, List<IDtoTypeMetadata>> metadatas)
    {
        LogHelper.Logger.Debug("Apply rules for all metadatas");
        foreach (var updater in _allMetadataUpdaters)
            updater.UpdateAllMetadata(metadatas);
    }
}