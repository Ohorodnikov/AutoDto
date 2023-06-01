using AutoDto.SourceGen.DtoAttributeData;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.SourceValidation;

internal interface ISourceValidatorFactory
{
    ISourceValidator Create(Compilation compilation);
}

internal class SourceValidatorFactory : ISourceValidatorFactory
{
    private IAttributeDataReader _attributeDataReader;
    public SourceValidatorFactory(IAttributeDataReader attributeDataReader)
    {
        _attributeDataReader = attributeDataReader;
    }
    public ISourceValidator Create(Compilation compilation)
    {
        return new SourceValidator(_attributeDataReader, compilation);
    }
}
