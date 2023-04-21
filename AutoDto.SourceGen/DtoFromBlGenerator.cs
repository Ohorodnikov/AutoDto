using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using AutoDto.Setup;
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
    private Debouncer<ExcecutorData> _debouncer;

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

    private class ExcecutorData
    {
        public ExcecutorData(SourceProductionContext context, List<ClassData> classes)
        {
            Context = context;
            Classes = classes;
        }

        public SourceProductionContext Context { get; }
        public List<ClassData> Classes { get; }
    }

    private static int id = 0;
    private int currId = 0;
    public DtoFromBlGenerator()
    {
         currId = Interlocked.Increment(ref id);

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

    public double DebonceTimeInMilisecocnds { get; set; } = 500;
    public bool AllowMultiInstance { get; set; } = false;

    private static object _lock = new object();    

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (!AllowMultiInstance && currId != 1)
            return;

        var path = Path.Combine(Environment.CurrentDirectory, "Logs");
        Directory.CreateDirectory(path);

        //if (LogHelper.Logger is null)
        LogHelper.InitFileLogger(path);

        if (_debouncer != null)
            return;

        lock (_lock)
        {
            if (_debouncer != null)
                return;

            _debouncer = new Debouncer<ExcecutorData>(ApplyGenerator, TimeSpan.FromMilliseconds(DebonceTimeInMilisecocnds));
        }

        context.RegisterSourceOutput(
                CreateSyntaxProvider(context),
                (ctx, classes) => _debouncer.RunAction(new ExcecutorData(ctx, classes.ToList())));

        LogHelper.Logger.Debug("Inited");
    }

    private IncrementalValueProvider<ImmutableArray<ClassData>> CreateSyntaxProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) =>
                {
                    //_logger.Log("Check node: " + node.GetType().Name);
                    if (node is not TypeDeclarationSyntax td)
                        return false;

                    //_logger.Log("Check: " + td.Identifier.ToString());
                    return true;
                },
                (sc, _) =>
                {
                    //_logger.Log("Collect: " + sc.ToTypeSymbol().Name);
                    return new ClassData(sc.ToTypeSymbol(), (TypeDeclarationSyntax)sc.Node);
                })
            .Where(x => _parser.CanParse(x.TypeDeclaration))
            .Collect();
    }

    private void ApplyGenerator(ExcecutorData data)
    {
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

        //_logger.Log("Add source " + $"{metadata.Name}.g.cs");

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
