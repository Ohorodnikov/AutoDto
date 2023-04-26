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
        LogHelper.Logger.Information("Apply RelationStrategyApplier");

        foreach (var metadata in metadatas.SelectMany(x => x.Value))
        {
            var applier = FindStrategyApplier(metadata.RelationStrategy);

            LogHelper.Logger.Debug("Apply {applier} for type {dtoType}", applier.GetType().Name, metadata.Name);

            applier.Apply(metadata, metadatas);
        }   
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
