using AutoDto.Setup;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;
using AutoDto.SourceGen.TypeParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;

internal class RelationStrategyApplier : IAllMetadatasUpdater
{
    public void UpdateAllMetadata(Dictionary<string, List<IDtoTypeMetadata>> metadatas)
    {
        foreach (var metadata in metadatas.SelectMany(x => x.Value))
            FindStrategyApplier(metadata.RelationStrategy).Apply(metadata, metadatas);
    }

    private IStrategyApplier FindStrategyApplier(RelationStrategy relationStrategy)
    {
        return relationStrategy switch
        {
            RelationStrategy.None => new NoneStrategyApplier(),
            RelationStrategy.ReplaceToIdProperty => new ReplaceToIdStrategyApplier(),
            RelationStrategy.AddIdProperty => new AddIdStrategyApplier(),
            RelationStrategy.ReplaceToDtoProperty => new ReplaceToDtoStrategyApplier(),
            _ => throw new InvalidOperationException()
        };
    }
}
