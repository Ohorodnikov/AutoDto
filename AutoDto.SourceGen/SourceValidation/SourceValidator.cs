using AutoDto.SourceGen.DtoAttributeData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.SourceValidation;

internal interface ISourceValidator
{
    bool IsValid(IEnumerable<INamedTypeSymbol> symbols);
}

internal class SourceValidator : ISourceValidator
{
    private readonly IAttributeDataReader _attributeDataReader;
    private readonly Compilation _compilation;

    public SourceValidator(IAttributeDataReader attributeDataReader, Compilation compilation)
    {
        _attributeDataReader = attributeDataReader;
        _compilation = compilation;
    }

    public bool IsValid(IEnumerable<INamedTypeSymbol> dtosSymbols)
    {
        var filesWithError = GetFilesWithErrors();

        if (filesWithError.Count == 0)
            return true;

        var skipFileNameCheck = filesWithError.Any(x => string.IsNullOrEmpty(x));

        bool IsFileInErrorList(string filePath) => skipFileNameCheck || filesWithError.Contains(filePath);

        foreach (var dtoSymbol in dtosSymbols)
        {
            var blSymbol = GetBlTypeSymbol(dtoSymbol);
            var allSymbols = Enumerable.Union(
                                            GetHierarchyTypesSymbols(dtoSymbol),
                                            GetHierarchyTypesSymbols(blSymbol));

            var valid = allSymbols.All(s => IsTypeSymbolValid(s, IsFileInErrorList));
            if (!valid)
                return false;
        }

        return true;
    }

    private List<string> GetFilesWithErrors()
    {
        var filesWithError = _compilation
                            .GetDiagnostics()
                            .Where(x => x.Severity == DiagnosticSeverity.Error)
                            .Select(x => x.Location.SourceTree.FilePath)
                            .Distinct()
                            .ToList();

        return filesWithError;
    }

    private bool IsTypeSymbolValid(ITypeSymbol typeSymbol, Func<string, bool> isFileWithError)
    {
        if (typeSymbol == null)
            return true;

        var srs = typeSymbol.DeclaringSyntaxReferences;

        if (srs == null)
            return true;        

        foreach (var sr in srs)
        {
            if (sr.SyntaxTree == null) 
                continue;

            if (!isFileWithError(sr.SyntaxTree.FilePath))
                continue;
            
            if (!IsSyntaxRefValid(sr))
                return false;
        }

        return true;
    }

    private bool IsSyntaxRefValid(SyntaxReference syntaxReference)
    {
        var sm = _compilation.GetSemanticModel(syntaxReference.SyntaxTree);

        var nodeSpan = syntaxReference.Span;

        var diagnostics = Enumerable.Union(
                            sm.GetDeclarationDiagnostics(nodeSpan), 
                            sm.GetSyntaxDiagnostics(nodeSpan));

        return !diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);
    }

    private INamedTypeSymbol GetBlTypeSymbol(ITypeSymbol dtoTypeSymbol)
    {
        foreach (var attr in dtoTypeSymbol.GetAttributes())
            if (_attributeDataReader.TryRead(attr, out var data) && data is DtoFromData dtoFrom)
                return dtoFrom.BlTypeSymbol;

        return null;
    }

    private IEnumerable<ITypeSymbol> GetHierarchyTypesSymbols(ITypeSymbol typeSymbol)
    {
        while (typeSymbol != null)
        {
            yield return typeSymbol;
            typeSymbol = typeSymbol.BaseType;
        }
    }
}
