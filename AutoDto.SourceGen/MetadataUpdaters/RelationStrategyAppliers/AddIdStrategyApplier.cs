using AutoDto.SourceGen.Metadatas;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal class AddIdStrategyApplier : OneMetadataStrategyApplier
{
    protected override void Apply(IDtoTypeMetadata metadata, List<(IPropertyMetadata relation, IPropertyMetadata idProp)> propsWithId)
    {
        foreach (var prop in propsWithId)
            metadata.Properties.Add(prop.idProp);
    }
}

