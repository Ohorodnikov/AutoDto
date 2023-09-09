using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.TypeParser;
using System;
using System.Collections.Generic;
using System.Text;
using AutoDto.SourceGen.DtoAttributeData;

namespace AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

internal abstract class BaseDtoFromMetadataUpdater<TData> 
    : AttributeMetadataUpdater<TData>
    where TData : DtoFromData
{
    public override void Update(IDtoTypeMetadata metadata, TData attributeData)
    {
        metadata.RelationStrategy = attributeData.RelationStrategy;
        metadata.BlFullName = attributeData.BlTypeSymbol.ToDisplayString();

        foreach (var property in attributeData.Properties)
            metadata.Properties.Add(new PropertyMetadata(property));
    }
}

internal class DtoFromMetadataUpdater : BaseDtoFromMetadataUpdater<DtoFromData> { }

internal class DtoForMetadataUpdater : BaseDtoFromMetadataUpdater<DtoForData> { }
