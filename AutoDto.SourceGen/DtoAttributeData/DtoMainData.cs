using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal class DtoMainData : DtoAttributeData
{
    public DtoMainData()
    {
        IsMain = true;
    }
    public bool IsMain { get; }

    protected override Action<TypedConstant>[] InitActions()
    {
        throw new NotImplementedException();
    }
}
