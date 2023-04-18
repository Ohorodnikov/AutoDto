using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.Metadatas;

public enum CollectionType
{
    NonCollection,
    Enumerable,
    Array
}

internal interface IPropertyMetadata
{
    Accessibility Accessibility { get; }
    bool IsReference { get; }
    string Name { get; }
    ITypeSymbol Type { get; }
    string TypeName { get; }

    CollectionType GetCollectionType();
    string GetElementTypeName();
    string ToPropertyString();
}

