using System.Collections.Generic;
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
public class DtoFromBlGenerator : ISourceGenerator
{
    private ITypeParser _parser;
    private IMetadataUpdaterHelper _updaterHelper;

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

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver(_parser));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            return;

        var dtoTypeMetadatas = CreateMetadata(receiver.DtoDeclarations, context);

        _updaterHelper.ApplyCommonRules(dtoTypeMetadatas);

        foreach (var dtoMetadata in dtoTypeMetadatas.SelectMany(x => x.Value))
        {
            WriteMessages(dtoMetadata, context);

            AddSourceIfNoErrors(dtoMetadata, context);
        }
    }

    private Dictionary<string, List<IDtoTypeMetadata>> CreateMetadata(List<SyntaxNode> dtoDeclarations, GeneratorExecutionContext context)
    {
        var dtoTypeMetadatas = new Dictionary<string, List<IDtoTypeMetadata>>(dtoDeclarations.Count);

        foreach (var declaration in dtoDeclarations)
        {
            var semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);
            var dtoSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(declaration);

            var md = _parser.Parse(dtoSymbol, declaration);

            var currentBlTypeStore = dtoTypeMetadatas.GetOrAdd(md.BlFullName, () => new List<IDtoTypeMetadata>());

            currentBlTypeStore.Add(md);
        }

        return dtoTypeMetadatas;
    }

    private void AddSourceIfNoErrors(IDtoTypeMetadata metadata, GeneratorExecutionContext context)
    {
        if (metadata.DiagnosticMessages.Any(msg => msg.message.Severity == DiagnosticSeverity.Error))
            return;

        context.AddSource($"{metadata.Name}.g.cs", SourceText.From(metadata.ToClassString(), Encoding.UTF8));
    }

    private void WriteMessages(IDtoTypeMetadata metadata, GeneratorExecutionContext context)
    {
        foreach (var message in metadata.DiagnosticMessages)
        {
            var location = message.location ?? metadata.Location ?? Location.None;
            var msg = message.message;
            context.ReportDiagnostic(Diagnostic.Create(msg.AsDiagnosticDescriptor(), location, msg.Parameters));
        }
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        private readonly ITypeParser _parser;
        public SyntaxReceiver(ITypeParser typeParser)
        {
            _parser = typeParser;
        }

        public List<SyntaxNode> DtoDeclarations { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (_parser.CanParse(syntaxNode))
                DtoDeclarations.Add(syntaxNode);
        }
    }
}
