using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.CompilerMessages;

public class MoreThanOneDtoAttrTests : BaseCompilerMessageTests
{
    [Fact]
    public void TwoIndependentAttributesSyntax()
    {
        var attrNamespace = typeof(DtoFromAttribute).Namespace;
        var code = $@"
using {attrNamespace};

namespace AutoDto.Tests.Dto;

public class SomeBlClass
{{
    public int Id {{ get; set; }}
}}

[DtoFrom(typeof(SomeBlClass))]
[DtoFor(typeof(SomeBlClass))]
{DtoCreator.GetPublicDtoDef("MyDto")} {{ }}
";

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(msgs);

        var expected = new MoreThanOneDtoDeclarationAttributeError();

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }

    [Fact]
    public void TwoAttributesAsOneSyntax()
    {
        var attrNamespace = typeof(DtoFromAttribute).Namespace;
        var code = $@"
using {attrNamespace};

namespace AutoDto.Tests.Dto;

public class SomeBlClass
{{
    public int Id {{ get; set; }}
}}

[DtoFrom(typeof(SomeBlClass)), DtoFor(typeof(SomeBlClass))]
{DtoCreator.GetPublicDtoDef("MyDto")} {{ }}
";

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(msgs);

        var expected = new MoreThanOneDtoDeclarationAttributeError();

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }
}
