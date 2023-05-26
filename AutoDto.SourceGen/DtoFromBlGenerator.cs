using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using AutoDto.Setup;
using AutoDto.SourceGen.Configuration;
using AutoDto.SourceGen.Debounce;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.SourceGen.DtoAttributeData;
using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.MetadataUpdaters;
using AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;
using AutoDto.SourceGen.MetadataUpdaters.OneMetadataUpdaters;
using AutoDto.SourceGen.SourceValidation;
using AutoDto.SourceGen.TypeParser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using Serilog.Events;

namespace AutoDto.SourceGen;

[Generator]
public class DtoFromBlGenerator : IIncrementalGenerator
{
    private ITypeParser _parser;
    private IMetadataUpdaterHelper _updaterHelper;
    private IAttributeDataReader _attributeDataReader;
    private ISourceValidator _sourceValidator;

    private class ClassData
    {
        public ClassData(INamedTypeSymbol typeSymbol, TypeDeclarationSyntax typeDeclaration, Compilation compilation)
        {
            TypeSymbol = typeSymbol;
            TypeDeclaration = typeDeclaration;
            Compilation = compilation;
        }

        public INamedTypeSymbol TypeSymbol { get; }
        public TypeDeclarationSyntax TypeDeclaration { get; }
        public Compilation Compilation { get; }
    }

    private class ExecutorData
    {
        public ExecutorData(SourceProductionContext context, List<ClassData> classes)
        {
            Context = context;
            Classes = classes;
        }

        public SourceProductionContext Context { get; }
        public List<ClassData> Classes { get; }
    }

    private static int id = 0;
    private int currId = 0;

    public DtoFromBlGenerator() : this(false, () => { })
    { }

    public DtoFromBlGenerator(bool allowMultiInstance, Action onExecute)
    {
        currId = Interlocked.Increment(ref id);

        _updaterHelper = new MetadataUpdaterHelper(
            new AttributeUpdaterFactory(),
            new IAllMetadatasUpdater[]
            {
                new RelationStrategyApplier(),
                new ConflictNamesChecker()
            });

        _attributeDataReader = new AttributeDataReader(new AttributeDataFactory());

        _sourceValidator = new SourceValidator(_attributeDataReader);

        _parser = new TypeParser.TypeParser(
            _attributeDataReader,
            _updaterHelper
            );

        _allowMultiInstance = allowMultiInstance;
        _onExecute = onExecute;
    }

    private bool _allowMultiInstance;
    private readonly Action _onExecute;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (!_allowMultiInstance && currId != 1)
            return;

        context.RegisterSourceOutput(
                CreateSyntaxProvider(context).Combine(context.AnalyzerConfigOptionsProvider),
                (ctx, data) => 
                {
                    try
                    {
                        Execute(ctx, data.Left, data.Right);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Logger?.Error(e, "Unhandled error");
                    }
                });

        LogHelper.Log(LogEventLevel.Information, "Generator inited in process {id}", Process.GetCurrentProcess().Id);
    }

    private void Execute(SourceProductionContext ctx, ImmutableArray<ClassData> classes, AnalyzerConfigOptionsProvider analyzer)
    {
        if (classes == null || classes.Length == 0)
            return;

        InitOptions(classes, analyzer);

        if (!_sourceValidator.IsSourcesValid(classes[0].Compilation, classes.Select(x => x.TypeSymbol)))
        {
            LogHelper.Log(LogEventLevel.Warning, "Do not run generation as one from dtos is compiled with errors");
            return;
        }

        DebouncerFactory<ExecutorData>
            .GetForAction(ApplyGenerator, GlobalConfig.Instance.DebouncerConfig)
            .RunAction(new ExecutorData(ctx, classes.ToList()))
            .Wait()
            ;
    }

    private void InitOptions(ImmutableArray<ClassData> classes, AnalyzerConfigOptionsProvider analyzer)
    {
        if (GlobalConfig.GlobalOptions == null)
            GlobalConfig.GlobalOptions = analyzer.GlobalOptions;

        if (GlobalConfig.Instance.IsInited)
        {
            LogHelper.Log(LogEventLevel.Verbose, "Options are already inited");
            return;
        }

        var editorValues = analyzer.GetOptions(classes[0].TypeDeclaration.SyntaxTree);

        GlobalConfig.Instance.Init(editorValues);
    }

    private IncrementalValueProvider<ImmutableArray<ClassData>> CreateSyntaxProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) =>
                {
                    LogHelper.Log(LogEventLevel.Verbose, "Check node '{node}' in {location}", node.GetType().Name, node.GetLocation().ToString());
                    if (node is not TypeDeclarationSyntax td)
                        return false;

                    return true;
                },
                (sc, ct) =>
                {
                    LogHelper.Log(LogEventLevel.Verbose, "Collect: {node}", sc.ToTypeSymbol().Name);
                    return new ClassData(sc.ToTypeSymbol(), (TypeDeclarationSyntax)sc.Node, sc.SemanticModel.Compilation);
                })
            .Where(x => _parser.CanParse(x.TypeDeclaration))
            .Collect();
    }

    private void ApplyGenerator(ExecutorData data)
    {
        _onExecute();
        LogHelper.Logger.Verbose("#### Start generator executing ####");

        if (data == null)
        {
            LogHelper.Logger.Warning("Cannot apply: data is null");
            return;
        }

        var classes = data.Classes;
        var context = data.Context;
        LogHelper.Logger.Information("Apply for {count} classes", classes.Count);
        var dtoTypeMetadatas = CreateMetadata(classes);

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
            LogHelper.Logger.Verbose("Create metadata for class {class}", classData.TypeSymbol.Name);

            var md = _parser.Parse(classData.TypeSymbol, classData.TypeDeclaration);

            LogHelper.Logger.Debug("Successfully parsed {dtoType} with master type {blType}", md.Name, md.BlFullName);

            dtoTypeMetadatas
                .GetOrAdd(md.BlFullName, () => new List<IDtoTypeMetadata>())
                .Add(md);
        }

        return dtoTypeMetadatas;
    }

    private void AddSourceIfNoErrors(IDtoTypeMetadata metadata, SourceProductionContext context)
    {
        if (metadata.DiagnosticMessages.Any(msg => msg.message.Severity == DiagnosticSeverity.Error))
            return;

        var name = $"{metadata.Name}.g.cs";
        LogHelper.Logger.Debug("Add source {name}", name);

        context.AddSource(name, SourceText.From(metadata.ToClassString(), Encoding.UTF8));
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
