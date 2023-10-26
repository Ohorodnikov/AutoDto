using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.SourceGeneration;

public class IgnorePropTest : BaseUnitTest
{
    const string DESCRIPTION = "Description";
    const string IGNORE_DATE_TIME = "IgnoreDateTime";

    [Theory]

    [InlineData("")]

    [InlineData(IGNORE_DATE_TIME)]
    [InlineData(DESCRIPTION)]
    [InlineData("SomeInvalidPropName")]

    [InlineData(IGNORE_DATE_TIME, DESCRIPTION)]
    [InlineData(IGNORE_DATE_TIME, "Invalid2")]
    [InlineData("SomeInvalidPropName", "Invalid2")]
    public void IgnorePropsFromAttributeTest(params string[] ignoredProps)
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name", typeof(string)).Build(),
            new PropertyBuilder(DESCRIPTION, typeof(string)).Build(),
            new PropertyBuilder(IGNORE_DATE_TIME, typeof(DateTime)).Build(),
        };

        var blClass =
            new ClassBuilder("IgnorePropModel")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute), string.Join(", ", ignoredProps.Select(WrapInQuotes)))
            .Build();

        var expectedPropsInDto = blMembers.Where(x => !ignoredProps.Contains(x.Name)).ToArray();
        var invalidPropsToIgnore = ignoredProps.Where(x => !blMembers.Any(bl => bl.Name == x)).ToArray();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(invalidPropsToIgnore.Length, msgs.Length);

            var expMsg = new NotFoundPropertyInBlWarn("", dtoClass.Name);

            Assert.All(msgs, msg => Assert.Equal(DiagnosticSeverity.Warning, msg.Severity));
            Assert.All(msgs, msg => Assert.Equal(expMsg.Id, msg.Id));

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(expectedPropsInDto));
        }
    }

    private string WrapInQuotes(string text) => '"' + text + '"';

    [Fact]
    public void EmptyIgnoreAttributeTest()
    {
        var blClass =
            new ClassBuilder("IgnorePropModel")
            .SetNamespace(BlNamespace)
            .AddMembers(new Member[]
            {
                CommonProperties.Id_Int,
                new PropertyBuilder("Name", typeof(string)).Build(),
            })
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoIgnoreAttribute))
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new AttributeValueNotSetError();

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            Assert.Equal(2, compilation.SyntaxTrees.Count()); //input trees only
        }
    }
}
