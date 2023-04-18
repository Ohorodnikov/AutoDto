using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace AutoDto.SourceGen.TypeParser;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class PropertyMetadata : IPropertyMetadata
{
    private PropertyMetadata(string name, string typeName, Accessibility accessibility)
    {
        Name = name;
        Accessibility = accessibility;
        _typeName = typeName;
    }
    public PropertyMetadata(IPropertySymbol property)
    {
        Name = property.Name;
        Type = property.Type;
        Accessibility = property.DeclaredAccessibility;
        IsReference = false;

    }

    public string Name { get; private set; }
    public ITypeSymbol Type { get; private set; }
    private string _typeName;
    public string TypeName => Type?.ToFullNameDisplayString() ?? _typeName;
    public bool IsReference { get; }
    public Accessibility Accessibility { get; }

    #region create

    public static PropertyMetadata CreateObjectProperty(string name, string objType, Accessibility accessibility)
        => new PropertyMetadata(name, objType, accessibility);

    public static PropertyMetadata CreateObjectProperty(string name, ITypeSymbol objType, Accessibility accessibility)
        => CreateObjectProperty(name, objType.ToFullNameDisplayString(), accessibility);

    public static PropertyMetadata CreateArrayProperty(string name, string arrayElementType, Accessibility accessibility)
        => CreateObjectProperty(name, arrayElementType + "[]", accessibility);

    public static PropertyMetadata CreateArrayProperty(string name, ITypeSymbol arrayElementType, Accessibility accessibility)
        => CreateArrayProperty(name, arrayElementType.ToFullNameDisplayString(), accessibility);

    public static PropertyMetadata CreateGenericProperty(string name, string genericName, string elementType, Accessibility accessibility)
        => CreateObjectProperty(name, $"{genericName}<{elementType}>", accessibility);

    public static PropertyMetadata CreateGenericProperty(string name, string genericName, ITypeSymbol elementType, Accessibility accessibility)
        => CreateGenericProperty(name, genericName, elementType.ToFullNameDisplayString(), accessibility);

    #endregion

    public CollectionType GetCollectionType()
    {
        if (Type == null)
            throw new NotImplementedException();

        if (!Type.IsReferenceType || Type.SpecialType == SpecialType.System_String)
            return CollectionType.NonCollection;

        if (Type.TypeKind == TypeKind.Array)
            return CollectionType.Array;

        var isEnumer = Type.AllInterfaces.Any(x => x.SpecialType == SpecialType.System_Collections_IEnumerable);

        return isEnumer ? CollectionType.Enumerable : CollectionType.NonCollection;
    }

    public string GetElementTypeName()
    {
        ITypeSymbol elementType = null;

        switch (GetCollectionType())
        {
            case CollectionType.Enumerable:
                elementType = ((INamedTypeSymbol)Type).TypeArguments[0];
                break;
            case CollectionType.Array:
                elementType = ((IArrayTypeSymbol)Type).ElementType;
                break;
            case CollectionType.NonCollection:
                elementType = Type;
                break;
            default:
                throw new NotSupportedException();
        }

        return elementType.ToFullNameDisplayString();
    }

    public string ToPropertyString()
    {
        return $"{SyntaxFacts.GetText(Accessibility)} {TypeName} {Name} {{ get; set; }}";
    }

    private string GetDebuggerDisplay()
    {
        return $"{SyntaxFacts.GetText(Accessibility)} {TypeName} {Name}";
    }
}


