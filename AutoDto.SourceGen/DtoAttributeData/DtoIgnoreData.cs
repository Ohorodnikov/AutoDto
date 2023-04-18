using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal class DtoIgnoreData : DtoAttributeData
{
    protected override Action<TypedConstant>[] InitActions()
    {
        return new[]
        {
            ParseIgnoredProps
        };
    }
    public List<string> IgnoredProperties { get; } = new();

    private void ParseIgnoredProps(TypedConstant value)
    {
        var v = value.Values;

        if (v == null)
        {
            DiagnosticMessages.Add(new AttributeNullError());
            return;
        }

        if (v.Length == 0)
        {
            DiagnosticMessages.Add(new AttributeValueNotSetError());
            return;
        }

        foreach (var item in v)
        {
            var ignoredProp = (string)item.Value;
            if (string.IsNullOrEmpty(ignoredProp))
            {
                DiagnosticMessages.Add(new NotFoundPropertyInBlWarn("", ""));
                continue;
            }
            IgnoredProperties.Add(ignoredProp);
        }
    }
}
