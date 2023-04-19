using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.SourceGen.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;

internal class DtoIgnoreMetadataUpdater : AttributeMetadataUpdater<DtoIgnoreData>
{
    public override void Update(IDtoTypeMetadata metadata, DtoIgnoreData attributeData)
    {
        foreach (var prop2Ignore in attributeData.IgnoredProperties)
        {
            var removeItem = metadata.Properties.Find(x => x.Name == prop2Ignore);

            if (removeItem == null)
            {
                attributeData.DiagnosticMessages.Add(new NotFoundPropertyInBlWarn(prop2Ignore, metadata.BlFullName));
                continue;
            }

            metadata.Properties.Remove(removeItem);
        }

        metadata.Properties.RemoveAll(x => attributeData.IgnoredProperties.Contains(x.Name));
    }
}
