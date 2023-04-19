using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.MetadataUpdaters;
using AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;
using AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;
using AutoDto.SourceGen.TypeParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoDto.SourceGen;

[Generator]
public class DtoFromBlGenerator : IIncrementalGenerator
{
    private ITypeParser _parser;
    private IMetadataUpdaterHelper _updaterHelper;

    private class ClassData 
    {
        public ClassData(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeDeclaration)
        {
            TypeSymbol = typeSymbol;
            TypeDeclaration = typeDeclaration;
        }

        public INamedTypeSymbol TypeSymbol { get; }
        public TypeDeclarationSyntax TypeDeclaration { get; }
    }

    public DtoFromBlGenerator()
    {
        _updaterHelper = new MetadataUpdaterHelper(
            new AttributeUpdaterFactory(),
            new IAllMetadatasUpdater[]
            {
                new RelationStrategyApplier(),
                new ConflictNamesChecker()
            });

        _parser = new TypeParser.TypeParser(
            new AttributeDataReader(new AttributeDataFactory()),
            _updaterHelper
            );
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            CreateSyntaxProvider(context), 
            (ctx, classes) => ApplyGenerator(ctx, classes));
    }

    private IncrementalValueProvider<ImmutableArray<ClassData>> CreateSyntaxProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is TypeDeclarationSyntax,
                (sc, _) => new ClassData(sc.ToTypeSymbol(), (TypeDeclarationSyntax)sc.Node))
            .Where(x => _parser.CanParse(x.TypeDeclaration))
            .Collect();
    }

    private void ApplyGenerator(SourceProductionContext context, IEnumerable<ClassData> classes)
    {
        var dtoTypeMetadatas = CreateMetadata(classes.ToList());

        _updaterHelper.ApplyCommonRules(dtoTypeMetadatas);

        foreach (var dtoMetadata in dtoTypeMetadatas.SelectMany(x => x.Value))
        {
            WriteMessages(dtoMetadata, context);

            AddSourceIfNoErrors(dtoMetadata, context);
        }
    }

    private Dictionary<string, List<IDtoTypeMetadata>> CreateMetadata(List<ClassData> classes)
    {
        var dtoTypeMetadatas = new Dictionary<string, List<IDtoTypeMetadata>>(classes.Count);

        foreach (var classData in classes)
        {
            var md = _parser.Parse(classData.TypeSymbol, classData.TypeDeclaration);

            var currentBlTypeStore = dtoTypeMetadatas.GetOrAdd(md.BlFullName, () => new List<IDtoTypeMetadata>());

            currentBlTypeStore.Add(md);
        }

        return dtoTypeMetadatas;
    }

    private void AddSourceIfNoErrors(IDtoTypeMetadata metadata, SourceProductionContext context)
    {
        if (metadata.DiagnosticMessages.Any(msg => msg.message.Severity == DiagnosticSeverity.Error))
            return;

        context.AddSource($"{metadata.Name}.g.cs", SourceText.From(metadata.ToClassString(), Encoding.UTF8));
    }

    private void WriteMessages(IDtoTypeMetadata metadata, SourceProductionContext context)
    {
        foreach (var message in metadata.DiagnosticMessages)
        {
            var location = message.location ?? metadata.Location ?? Location.None;
            var msg = message.message;
            context.ReportDiagnostic(Diagnostic.Create(msg.AsDiagnosticDescriptor(), location, msg.Parameters));
        }
    }
}
