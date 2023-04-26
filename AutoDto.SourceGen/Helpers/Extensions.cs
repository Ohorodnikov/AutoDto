using AutoDto.SourceGen.DiagnosticMessages;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.Helpers;

internal static class Extensions
{
    public static INamedTypeSymbol ToTypeSymbol(this GeneratorSyntaxContext syntaxContext)
    {
        return (INamedTypeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
    }
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFunc)
    {
        if (dictionary.TryGetValue(key, out TValue value))
            return value;

        var v = valueFunc();
        dictionary.Add(key, v);

        return v;
    }

    public static string ToFullNameDisplayString(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters));
    }

    public static IEnumerable<ISymbol> GetAllMembersFromAllBaseTypes(this ITypeSymbol typeSymbol)
    {
        var members = new List<ISymbol>();
        if (typeSymbol == null)
            return members;

        members.AddRange(typeSymbol.GetMembers());
        members.AddRange(GetAllMembersFromAllBaseTypes(typeSymbol.BaseType));

        return members;
    }
}
