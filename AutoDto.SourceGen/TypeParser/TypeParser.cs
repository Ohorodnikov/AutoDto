using AutoDto.Setup;
using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.MetadataUpdaters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog.Events;

namespace AutoDto.SourceGen.TypeParser;

internal interface ITypeParser
{
    IDtoTypeMetadata Parse(INamedTypeSymbol namedType, SyntaxNode typeDeclaration);
    bool CanParse(SyntaxNode syntaxNode);
}

internal class TypeParser : ITypeParser
{
    private readonly IAttributeDataReader _attributeDataReader;
    private readonly IMetadataUpdaterHelper _updaterHelper;

    public TypeParser(IAttributeDataReader attributeDataReader, IMetadataUpdaterHelper updaterFactory)
    {
        _attributeDataReader = attributeDataReader;
        _updaterHelper = updaterFactory;
    }

    private static string[] _attributes = new[]
    {
        nameof(DtoFromAttribute),
        nameof(DtoForAttribute)
    };

    public bool CanParse(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax typeDeclaration)
            return false;

        if (typeDeclaration.AttributeLists == null || typeDeclaration.AttributeLists.Count == 0)
        {
            LogHelper.Log(LogEventLevel.Verbose, "Type {typeName} doesnt have attributes. It cannot be a dto to parse", typeDeclaration.Identifier.Text);
            return false;
        }

        var allAttrs = GetAllTypeAttributesNames(typeDeclaration);

        var hasAttr = allAttrs.Any(_attributes.Contains);

        LogHelper.Log(LogEventLevel.Verbose, "Type {typeName} can {not} be parsed", typeDeclaration.Identifier.Text, hasAttr ? "" : "NOT");

        return hasAttr;
    }

    public IDtoTypeMetadata Parse(INamedTypeSymbol namedType, SyntaxNode typeDeclaration)
    {
        if (typeDeclaration is not TypeDeclarationSyntax td)
            throw new InvalidOperationException();

        var metadata = new DtoTypeMetadata
        {
            Name = namedType.Name,
            Namespace = namedType.ContainingNamespace.ToDisplayString(),
            Accessibility = namedType.DeclaredAccessibility,
            IsMain = false,
            TypeSymbol = namedType,
            TypeDeclaration = td,
            Location = td.GetLocation(),
        };

        var validationResult = ValidateDeclaration(metadata);
        var hasErrors = false;
        foreach (var diag in validationResult)
        {
            metadata.DiagnosticMessages.Add((metadata.Location, diag));
            if (diag.Severity == DiagnosticSeverity.Error)
                hasErrors = true;
        }

        if (!hasErrors)
        {
            var attrDatas = ReadAttributes(namedType);

            _updaterHelper.InitByAttributes(metadata, attrDatas);
        }

        return metadata;
    }

    private List<IDtoAttributeData> ReadAttributes(INamedTypeSymbol namedType)
    {
        var attrDatas = new List<IDtoAttributeData>();

        foreach (var item in namedType.GetAttributes())
        {
            LogHelper.Logger.Verbose("Process attribute {attrName} in type {typeName}", item.AttributeClass.ToDisplayString(), namedType.Name);
            if (_attributeDataReader.TryRead(item, out var data))
                attrDatas.Add(data);
        }   

        return attrDatas;
    }

    private IEnumerable<BaseDiagnosticMessage> ValidateDeclaration(IDtoTypeMetadata metadata)
    {
        var diag = new List<BaseDiagnosticMessage>();
        if (!IsTypeHasPartialKeyword(metadata.TypeDeclaration))
            diag.Add(new DtoNotPartialError(metadata.Name));

        if (!HasOnlyOneDtoDelarationAttribute(metadata.TypeDeclaration))
            diag.Add(new MoreThanOneDtoDeclarationAttributeError());

        return diag;
    }

    private bool IsTypeHasPartialKeyword(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    private bool HasOnlyOneDtoDelarationAttribute(TypeDeclarationSyntax typeDeclaration)
    {
        var attrs = GetAllTypeAttributesNames(typeDeclaration);

        var count = attrs.Count(_attributes.Contains);

        return count == 1;
    }

    private IEnumerable<string> GetAllTypeAttributesNames(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration
            .AttributeLists
            .SelectMany(x => x.Attributes)
            .Select(x => 
            {
                var name = x.Name.ToString();
                if (!name.EndsWith(nameof(Attribute)))
                    name += nameof(Attribute);

                return name;
            });
    }
}
