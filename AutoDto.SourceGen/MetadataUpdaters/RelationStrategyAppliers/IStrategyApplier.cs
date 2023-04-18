using AutoDto.SourceGen.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal interface IStrategyApplier
{
    void Apply(IDtoTypeMetadata metadata, Dictionary<string, List<IDtoTypeMetadata>> allMetadatas);
}