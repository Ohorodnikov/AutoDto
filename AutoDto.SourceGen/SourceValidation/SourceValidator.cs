using AutoDto.SourceGen.DtoAttributeData;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.SourceValidation;

internal interface ISourceValidator
{
    bool IsSourcesValid(Compilation compilation, IEnumerable<INamedTypeSymbol> symbols);
}

internal class SourceValidator : ISourceValidator
{
    private IAttributeDataReader _attributeDataReader;

    public SourceValidator(IAttributeDataReader attributeDataReader)
    {
        _attributeDataReader = attributeDataReader;
    }

    public bool IsSourcesValid(Compilation compilation, IEnumerable<INamedTypeSymbol> symbols)
    {
        var errors = compilation
                    .GetDiagnostics()
                    .Where(x => x.Severity == DiagnosticSeverity.Error)
                    .ToList();

        if (errors.Count == 0)
            return true;

        var filesWithError = errors
                            .Select(x => x.Location.SourceTree.FilePath)
                            .Distinct()
                            .ToList();

        var skipFileNameCheck = filesWithError.Any(x => string.IsNullOrEmpty(x));

        foreach (var symbol in symbols)
        {
            var allRelatedSyntaxTrees = GetInheritanceSyntaxTrees(symbol).Union(GetBlInheritance(symbol));

            foreach (var syntaxTree in allRelatedSyntaxTrees)
            {
                var filePath = syntaxTree.FilePath;
                LogHelper.Logger.Debug("Check syntaxTree in file {filePath} on compilation errors", filePath);
                if (skipFileNameCheck || filesWithError.Contains(filePath))
                {
                    var isValidSyntax = IsSyntaxValid(compilation, syntaxTree);
                    LogHelper.Logger.Error("Checked syntax tree in file {filePath} is {status}", filePath, isValidSyntax);

                    if (!isValidSyntax)
                        return false; //Return on first found not compiled code
                }
            }
        }

        return true;
    }

    private bool IsSyntaxValid(Compilation compilation, SyntaxTree syntaxTree)
    {
        var sm = compilation.GetSemanticModel(syntaxTree);

        var diagnostics = sm.GetDeclarationDiagnostics().Union(sm.GetSyntaxDiagnostics());

        return !diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);
    }

    private IEnumerable<SyntaxTree> GetInheritanceSyntaxTrees(ITypeSymbol typeSymbol)
    {
        while (typeSymbol != null)
        {
            foreach (var loc in typeSymbol.Locations)
                if (loc.SourceTree != null)
                {
                    LogHelper.Logger.Debug("Find syntax tree for {name} in location {location}", typeSymbol.Name, loc);
                    yield return loc.SourceTree;
                }

            typeSymbol = typeSymbol.BaseType;
        }
    }

    private IEnumerable<SyntaxTree> GetBlInheritance(ITypeSymbol typeSymbol)
    {
        foreach (var attr in typeSymbol.GetAttributes())
            if (_attributeDataReader.TryRead(attr, out var data) && data is DtoFromData dtoFrom)
                return GetInheritanceSyntaxTrees(dtoFrom.BlTypeSymbol);

        return Enumerable.Empty<SyntaxTree>();
    }
}
