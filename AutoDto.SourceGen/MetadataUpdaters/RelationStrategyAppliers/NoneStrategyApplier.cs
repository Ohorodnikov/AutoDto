using AutoDto.SourceGen.Metadatas;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal class NoneStrategyApplier : IStrategyApplier
{
    public void Apply(IDtoTypeMetadata metadata, Dictionary<string, List<IDtoTypeMetadata>> allMetadatas)
    { }
}

