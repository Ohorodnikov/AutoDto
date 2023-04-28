using AutoDto.Setup;
using AutoDto.SourceGen;
using AutoDto.Tests.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;

namespace AutoDto.Tests;

public abstract class BaseUnitTest
{
    public BaseUnitTest()
    {
        SyntaxChecker = new SyntaxChecker();
        DtoCreator = new DtoCodeCreator();
        Generator = new GeneratorRunner();

        LogHelper.InitDebugLogger();
    }

    protected SyntaxChecker SyntaxChecker { get; }
    protected DtoCodeCreator DtoCreator { get; }
    protected GeneratorRunner Generator { get; }

    protected GeneratorRunner GetGeneratorConfigured(bool checkInputCompilation, Action onRun)
    {
        return new GeneratorRunner
        {
            CheckInputCompilation = checkInputCompilation,
            OnApplyGenerator = onRun
        };
    }

    protected Compilation RunForDtos(params DtoData[] dtos)
    {
        var code = DtoCreator.GetDtosDefinition(dtos);

        return Generator.Run(code);
    } 

}
