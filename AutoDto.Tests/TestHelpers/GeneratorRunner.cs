﻿using AutoDto.Setup;
using AutoDto.SourceGen;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.TestHelpers;

public class GeneratorRunner
{
    public Action OnApplyGenerator { get; set; } = () => { };
    public bool CheckInputCompilation { get; set; } = true;
    public Compilation Run(string code, IEnumerable<MetadataReference> extraRefs = null)
    {
        return RunWithMsgs(code, extraRefs).compilation;
    }

    public (Compilation compilation, ImmutableArray<Diagnostic> compileMsgs) RunWithMsgs(IEnumerable<string> codes, IEnumerable<MetadataReference> extraRefs = null)
    {
        extraRefs ??= new List<MetadataReference>();
        var systemAssemblyRefs = GetSystemRefs();
        var commonRefs = GetCommonRefs();

        var allRefs = systemAssemblyRefs.Union(commonRefs).Union(extraRefs);

        var compilation = CSharpCompilation.Create(
            "MyCompilation",
            syntaxTrees: codes.Select(code => CSharpSyntaxTree.ParseText(code, path: Guid.NewGuid().ToString() + ".cs")).ToList(),
            references: allRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (CheckInputCompilation)
            Compile(compilation); //to see compile errs if any in code

        var driver = CSharpGeneratorDriver.Create(new[] { new DtoFromBlGenerator(true, OnApplyGenerator) });

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return (outputCompilation, diagnostics);
    }

    public (Compilation compilation, ImmutableArray<Diagnostic> compileMsgs) RunWithMsgs(string code, IEnumerable<MetadataReference> extraRefs = null)
    {
        return RunWithMsgs(new[] {code}, extraRefs);
    }

    public IEnumerable<MetadataReference> GetSystemRefs()
    {
        var assemblies = new[]
        {
            typeof(object).Assembly
        };

        var returnList = CreateFromAsms(assemblies);

        //The location of the .NET assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        /* 
            * Adding some necessary .NET assemblies
            * These assemblies couldn't be loaded correctly via the same construction as above,
            * in specific the System.Runtime.
            */

        var libs = new[]
        {
            "mscorlib.dll",
            "netstandard.dll",

            "System.dll",
            "System.Core.dll",
            "System.Runtime.dll",
            "System.Collections.dll",
            "System.Collections.Concurrent.dll",
            "System.Collections.Immutable.dll",
        };

        foreach (var lib in libs)
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath,lib)));

        return returnList;
    }

    public IEnumerable<MetadataReference> GetCommonRefs()
    {
        var asms = new[]
        {
            typeof(BaseUnitTest).Assembly,
            typeof(DtoFromAttribute).Assembly,

            typeof(Serilog.ILogger).Assembly,
            typeof(Serilog.LoggerSinkConfigurationDebugExtensions).Assembly,
            typeof(Serilog.FileLoggerConfigurationExtensions).Assembly,

        };

        return CreateFromAsms(asms);
    }

    public List<PortableExecutableReference> CreateFromAsms(IEnumerable<Assembly> asms)
    {
        return asms.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();
    }

    public void Compile(CSharpCompilation compilation)
    {
        using (var stream = new FileStream($"../../../GeneratorInputDlls/{Guid.NewGuid()}.dll", FileMode.Create))
        {
            var result = compilation.Emit(stream);
            if (!result.Success)
            {
                throw new Exception(string.Join("; ", result.Diagnostics.Select(x => x.ToString())));
            }
            // Handle compilation errors if any
        }
    }
}

