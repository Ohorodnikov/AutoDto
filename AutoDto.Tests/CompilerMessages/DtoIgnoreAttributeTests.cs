using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.CompilerMessages;

public class DtoIgnoreAttributeTests : BaseCompilerMessageTests
{
    [Fact]
    public void CorrectPropNameTest()
    {
        var blName = "IgnorePropModel";
        var ignorePropName = "IgnoreDateTime";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(ignorePropName, typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), WrapInQuotes(ignorePropName))
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(3, compilation.SyntaxTrees.Count());
            Assert.Empty(msgs);
        }
    }

    [Fact]
    public void IncorrectPropNameTest()
    {
        var blName = "IgnorePropModel";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder("IgnoreDateTime", typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), WrapInQuotes("InvalidProp"))
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(3, compilation.SyntaxTrees.Count());
            Assert.Single(msgs);

            var expected = new NotFoundPropertyInBlWarn("", "");
            AssertMessage(DiagnosticSeverity.Warning, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void EmptyStringValueTest()
    {
        var blName = "IgnorePropModel";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder("IgnoreDateTime", typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), WrapInQuotes(string.Empty))
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(3, compilation.SyntaxTrees.Count());
            Assert.Single(msgs);

            var expected = new NotFoundPropertyInBlWarn("", "");
            AssertMessage(DiagnosticSeverity.Warning, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void NoArgumentTest()
    {
        var blName = "IgnorePropModel";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder("IgnoreDateTime", typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), string.Empty)
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(2, compilation.SyntaxTrees.Count());
            Assert.Single(msgs);

            var expected = new AttributeValueNotSetError();

            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void NullValueTest()
    {
        var blName = "IgnorePropModel";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder("IgnoreDateTime", typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), "null")
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(2, compilation.SyntaxTrees.Count());
            Assert.Single(msgs);

            var expected = new AttributeNullError();
            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void NoBracketsTest()
    {
        var blName = "IgnorePropModel";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder("IgnoreDateTime", typeof(DateTime)).Build())
            .Build(),

            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute))
            .Build()
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(2, compilation.SyntaxTrees.Count());
            Assert.Single(msgs);

            var expected = new AttributeValueNotSetError();
            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }

    private string WrapInQuotes(string str) => '"' + str + '"';
}
