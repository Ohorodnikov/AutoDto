using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

internal interface IAttributeMetadataUpdater
{
    void Update(IDtoTypeMetadata metadata, IDtoAttributeData attributeData);
}

internal abstract class AttributeMetadataUpdater<TAttributeData> : IAttributeMetadataUpdater
    where TAttributeData : IDtoAttributeData
{
    public abstract void Update(IDtoTypeMetadata metadata, TAttributeData attributeData);

    public void Update(IDtoTypeMetadata metadata, IDtoAttributeData attributeData)
    {
        Update(metadata, (TAttributeData)attributeData);

        foreach (var diag in attributeData.DiagnosticMessages)
            metadata.DiagnosticMessages.Add((attributeData.Location, diag));
    }
}
