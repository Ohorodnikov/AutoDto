using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.TypeParser;
using Microsoft.CodeAnalysis;
using System.Collections;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal abstract class OneMetadataStrategyApplier : IStrategyApplier
{
    private IPropertyMetadata GetRelationMetadata(IPropertyMetadata prop)
    {
        switch (prop.GetCollectionType())
        {
            case CollectionType.NonCollection:
                return CreateClassMetadata(prop);
            case CollectionType.Enumerable:
                return CreateEnumerableMetadata(prop);
            case CollectionType.Array:
                return CreateArrayMetadata(prop);
            default:
                throw new NotSupportedException();
        }
    }

    private static IPropertyMetadata CreateEnumerableMetadata(IPropertyMetadata prop)
    {
        var type = (INamedTypeSymbol)prop.Type;

        var isEnumerable = type.AllInterfaces.Any(x => x.Name == nameof(IEnumerable));

        if (!isEnumerable)
            return null;

        if (type.TypeArguments.Length != 1)
            return null;

        if (!TryFindIdProperty(type.TypeArguments[0], out var idProperty))
            return null;

        var collectionName = type.ContainingNamespace + "." + type.Name;

        return PropertyMetadata.CreateGenericProperty(GetRelationName(prop.Name, true), collectionName, idProperty.Type, prop.Accessibility);
    }

    private IPropertyMetadata CreateClassMetadata(IPropertyMetadata prop)
    {
        if (!TryFindIdProperty(prop.Type, out var idProperty))
            return null;

        return PropertyMetadata.CreateObjectProperty(GetRelationName(prop.Name, false), idProperty.Type, idProperty.DeclaredAccessibility);
    }

    private IPropertyMetadata CreateArrayMetadata(IPropertyMetadata prop)
    {
        var type = (IArrayTypeSymbol)prop.Type;
        if (!TryFindIdProperty(type.ElementType, out var idProperty))
            return null;

        return PropertyMetadata.CreateArrayProperty(GetRelationName(prop.Name, true), idProperty.Type, prop.Accessibility);
    }

    private static bool TryFindIdProperty(ITypeSymbol propertyType, out IPropertySymbol idProperty)
    {
        idProperty = null;
        foreach (var member in propertyType.GetAllMembersFromAllBaseTypes().OfType<IPropertySymbol>())
        {
            if (member.Name == "Id")
            {
                idProperty = member;
                return true;
            }
        }
        return false;
    }

    private static string GetRelationName(string baseName, bool forCollection)
    {
        return baseName + (forCollection ? "Ids" : "Id");
    }

    private List<(IPropertyMetadata relation, IPropertyMetadata idProp)> GetRelationWithIdProp(IDtoTypeMetadata metadata)
    {
        var propsWithId = new List<(IPropertyMetadata relation, IPropertyMetadata idProp)>(metadata.Properties.Count);

        foreach (var prop in metadata.Properties)
        {
            var relMd = GetRelationMetadata(prop);

            if (relMd != null)
                propsWithId.Add((prop, relMd));
        }

        return propsWithId;
    }

    protected abstract void Apply(IDtoTypeMetadata metadata, List<(IPropertyMetadata relation, IPropertyMetadata idProp)> propsWithId);

    public void Apply(IDtoTypeMetadata metadata, Dictionary<string, List<IDtoTypeMetadata>> allMetadatas)
    {
        Apply(metadata, GetRelationWithIdProp(metadata));
    }
}

