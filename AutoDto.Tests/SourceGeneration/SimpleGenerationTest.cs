using AutoDto.Setup;
using AutoDto.Tests.SourceGeneration.Models;
using AutoDto.Tests.TestHelpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Text;
using Xunit;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class SimplePropGenerationTests : BaseUnitTest
{
    [Theory]
    [InlineData(typeof(TypeWith1IntProp))]
    [InlineData(typeof(TypeWith1StringProp))]
    [InlineData(typeof(TypeWith1DateTimeProp))]
    [InlineData(typeof(TypeWithMultipleProps))]
    public void TestAllPropsGeneration(Type type)
    {
        var dto = new DtoData(type, RelationStrategy.None, "MyDto");

        var compilation = RunForDtos(dto);

        var classSyntax = SyntaxChecker.FindClassByName(compilation, dto.DtoName);

        var expectedProps = type.GetPublicInstProperties().Select(x => new PropertyDescriptor(x));

        SyntaxChecker.TestOneClassDeclaration(classSyntax, expectedProps.ToArray());
    }
}
