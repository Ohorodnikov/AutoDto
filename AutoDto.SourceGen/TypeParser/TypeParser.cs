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

    public bool CanParse(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax typeDeclaration)
            return false;

        if (typeDeclaration.AttributeLists == null || typeDeclaration.AttributeLists.Count == 0)
        {
            LogHelper.Log(LogEventLevel.Verbose, "Type {typeName} doesnt have attributes. It cannot be a dto to parse", typeDeclaration.Identifier.Text);
            return false;
        }

        var allAttrs = typeDeclaration.AttributeLists.SelectMany(x => x.Attributes);

        var searchedAttr1 = nameof(DtoFromAttribute);
        var searchedAttr2 = searchedAttr1.Replace(nameof(Attribute), "");

        var hasAttr = allAttrs.Any(x =>
        {
            var name = x.Name.ToString();

            return name == searchedAttr1 || name == searchedAttr2;
        });

        if (hasAttr)
            LogHelper.Log(LogEventLevel.Verbose, "Type {typeName} can be parsed", typeDeclaration.Identifier.Text);
        else
            LogHelper.Log(LogEventLevel.Verbose, "Type {typeName} can NOT be parsed", typeDeclaration.Identifier.Text);

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

        ValidateDeclaration(metadata);

        var attrDatas = ReadAttributes(namedType);

        _updaterHelper.InitByAttributes(metadata, attrDatas);

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

    private void ValidateDeclaration(IDtoTypeMetadata metadata)
    {
        if (IsTypeHasPartialKeyword(metadata.TypeDeclaration))
            return;

        metadata.DiagnosticMessages.Add((metadata.Location, new DtoNotPartialError(metadata.Name)));
    }

    private bool IsTypeHasPartialKeyword(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }
}
