﻿using AutoDto.SourceGen;
using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests;

public abstract class BaseUnitTest
{
    public BaseUnitTest()
    {
        SyntaxChecker = new SyntaxChecker();
        DtoCreator = new DtoCodeCreator();
        Generator = new GeneratorRunner();

        LogHelper.InitDebugLogger();

        var currentTestNamespace = GetType().Namespace;

        DtoNamespace = $"{currentTestNamespace}.Models.Dtos";
        BlNamespace = $"{currentTestNamespace}.Models.Bls";
    }

    protected string DtoNamespace { get; }
    protected string BlNamespace { get; }

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


    protected void RunWithAssert(IEnumerable<ClassElement> classes, Action<Compilation, ImmutableArray<Diagnostic>> assertCallback)
    {
        RunWithAssert(classes, null, assertCallback);
    }

    protected void RunWithAssert(IEnumerable<ClassElement> classes, IEnumerable<MetadataReference> extraRefs, Action<Compilation, ImmutableArray<Diagnostic>> assertCallback)
    {
        var res = Generator.Run(classes, extraRefs);

        assertCallback(res.compilation, res.compileMsgs);
    }

    protected IEnumerable<PropertyDescriptor> Member2PropDescriptor(IEnumerable<Member> members)
    {
        return members.Select(x => new PropertyDescriptor(TypeMap[x.ReturnType], x.Name));
    }

    private static Dictionary<string, Type> TypeMap = new()
    {
        { "String", typeof(string) },
        { "int", typeof(int) },
        { "Int32", typeof(int) },
        { "DateTime", typeof(DateTime) },
        { "DateTimeKind", typeof(DateTimeKind) },
        { "Object", typeof(object) },
    };
}
