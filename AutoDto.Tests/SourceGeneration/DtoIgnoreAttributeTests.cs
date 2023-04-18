using AutoDto.Setup;
using AutoDto.Tests.SourceGeneration.Models;
using AutoDto.Tests.TestHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Runtime.InteropServices;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class IgnorePropTest : BaseUnitTest
{
    [Theory]

    //Ignore one prop
    [InlineData(nameof(IgnorePropModel.IgnoreDateTime))]
    [InlineData(nameof(IgnorePropModel.Description))]
    [InlineData("SomeInvalidPropName")]

    //Ignore many props
    [InlineData(nameof(IgnorePropModel.IgnoreDateTime), nameof(IgnorePropModel.Description))]
    [InlineData(nameof(IgnorePropModel.IgnoreDateTime), "Invalid2")]
    [InlineData("SomeInvalidPropName", "Invalid2")]
    public void IgnorePropsFromAttributeTest(params string[] ignoredProps)
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute);
        var attrParams = string.Join(", ", ignoredProps.Select(x => '"' + x + '"'));
        var attr = $"[{ignoreAttr.Name}({attrParams})]";
        RunTest(attr, ignoredProps);
    }

    [Fact]
    public void IgnoreEmptyStringAtrributeTest()
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute);
        var attrIgnoreDef = $"[{ignoreAttr.Name}(\"\")]";
        RunTest(attrIgnoreDef, new string[0]);
    }

    [Fact]
    public void EmptyIgnoreAttributeTest()
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute);

        var attrList = new List<(string nameSpace, string value)>
        {
            (ignoreAttr.Namespace, $"[{ignoreAttr.Name}]")
        };

        var dto = new DtoData(typeof(IgnorePropModel), RelationStrategy.None, "MyDto", attrList);

        var code = DtoCreator.GetDtosDefinition(dto);

        var trees = Generator.Run(code).SyntaxTrees;

        Assert.Equal(1, trees.Count());
    }

    private void RunTest(string attributeDef, string[] ignoredProps)
    {
        var ignoreAttr = typeof(DtoIgnoreAttribute);

        var attrList = new List<(string nameSpace, string value)>
        {
            (ignoreAttr.Namespace, attributeDef)
        };

        var dto = new DtoData(typeof(IgnorePropModel), RelationStrategy.None, "MyDto", attrList);
        var code = DtoCreator.GetDtosDefinition(dto);
        
        var compilation = Generator.Run(code);

        var genClass = SyntaxChecker.FindClassByName(compilation, dto.DtoName);

        var expectedProps = GetExpectedProperties(dto.Type, ignoredProps).Select(x => new PropertyDescriptor(x));

        SyntaxChecker.TestOneClassDeclaration(genClass, expectedProps);
    }

    private IEnumerable<PropertyInfo> GetExpectedProperties(Type type, string[] ignoredProps)
    {
        return type.GetPublicInstProperties().Where(p => !ignoredProps.Contains(p.Name));
    }
}
