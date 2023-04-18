using AutoDto.SourceGen.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;

internal interface IAllMetadatasUpdater
{
    void UpdateAllMetadata(Dictionary<string, List<IDtoTypeMetadata>> metadatas);
}
