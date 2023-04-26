using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.CompilerMessages.Models;
//using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.CompilerMessages;

public class DtoIgnoreAttributeTests : BaseCompilerMessageTests
{
    [Fact]
    public void CorrectPropNameTest()
    {
        var (trees, msgs) = RunTest(WripInQuotes(nameof(IgnorePropModel.Description)));

        Assert.Equal(2, trees.Count());

        Assert.Empty(msgs);
    }

    [Fact]
    public void IncorrectPropNameTest()
    {
        var invalidPropName = "InvalidProp";
        var (trees, msgs) = RunTest(WripInQuotes(invalidPropName));

        Assert.Equal(2, trees.Count());
        Assert.Single(msgs);

        var msg = msgs[0];
        Assert.Equal(DiagnosticSeverity.Warning, msg.Severity);

    }

    [Fact]
    public void EmptyStringValueTest()
    {
        var (trees, msgs) = RunTest(WripInQuotes(""));

        Assert.Equal(2, trees.Count());
        Assert.Single(msgs);

        var expected = new NotFoundPropertyInBlWarn("", "");

        AssertMessage(DiagnosticSeverity.Warning, expected.Id, msgs[0]);
    }

    [Fact]
    public void NoArgumentTest()
    {
        var (trees, msgs) = RunTest("");

        Assert.Single(trees);
        Assert.Single(msgs);

        var expected = new AttributeValueNotSetError();

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }

    [Fact]
    public void NullValueTest()
    {
        var (trees, msgs) = RunTest("null");

        Assert.Single(trees);
        Assert.Single(msgs);

        var expected = new AttributeNullError();

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }

    [Fact]
    public void NoBracketsTest()
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute).Name.Replace(nameof(Attribute), "");
        var attrIgnoreDef = $"[{ignoreAttr}]";

        var type = typeof(IgnorePropModel);
        var attr = DtoCreator.GetDtoFromAttr(type);

        var code = $@"
using {attr.nameSpace};
using {type.Namespace};

namespace AutoDto.Tests.Dto;

{attr.definition}
{attrIgnoreDef}
{DtoCreator.GetPublicDtoDef("MyDto")} {{ }}
";

        var result = Generator.RunWithMsgs(code);

        Assert.Single(result.compilation.SyntaxTrees);

        Assert.Single(result.compileMsgs);

        var expected = new AttributeValueNotSetError();

        AssertMessage(DiagnosticSeverity.Error, expected.Id, result.compileMsgs[0]);
    }

    private string WripInQuotes(string str) => '"' + str + "\"";

    private (IEnumerable<SyntaxTree> syntaxTrees, ImmutableArray<Diagnostic> messages) RunTest(string ignoreAttrValue)
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute);
        var attrIgnoreDef = $"[{ignoreAttr.Name}({ignoreAttrValue})]";

        var type = typeof(IgnorePropModel);
        var attr = DtoCreator.GetDtoFromAttr(type);

        var code = $@"
using {attr.nameSpace};
using {type.Namespace};

namespace AutoDto.Tests.Dto;

{attr.definition}
{attrIgnoreDef}
{DtoCreator.GetPublicDtoDef("MyDto")} {{ }}
";

        var result = Generator.RunWithMsgs(code);

        return (result.compilation.SyntaxTrees, result.compileMsgs);
    }
}
