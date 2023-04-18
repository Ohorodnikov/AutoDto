using AutoDto.SourceGen.Metadatas;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal class ReplaceToIdStrategyApplier : OneMetadataStrategyApplier
{
    protected override void Apply(IDtoTypeMetadata metadata, List<(IPropertyMetadata relation, IPropertyMetadata idProp)> propsWithId)
    {
        foreach (var prop in propsWithId)
        {
            metadata.Properties.Remove(prop.relation);
            metadata.Properties.Add(prop.idProp);
        }
    }
}

