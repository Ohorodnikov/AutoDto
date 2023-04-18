using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

internal class DtoMainMetadataUpdater : AttributeMetadataUpdater<DtoMainData>
{
    public override void Update(IDtoTypeMetadata metadata, DtoMainData attributeData)
    {
        metadata.IsMain = attributeData.IsMain;
    }
}
