using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class BaseMemberTests : BaseUnitTest
{
    protected void TestGeneratedMembersForType<T>()
    {
        var dto = new DtoData(typeof(T), Setup.RelationStrategy.None, "MemberTestsDto");

        var compilation = RunForDtos(dto);
        var generatedClass = SyntaxChecker.FindClassByName(compilation, dto.DtoName);

        var props = typeof(T)
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(x => x.CanRead)
                            .Where(x => x.GetMethod.IsPublic)
                            .ToArray();

        SyntaxChecker.TestOneClassDeclaration(generatedClass, props.Select(x => new PropertyDescriptor(x)));
    }
}

public class BlInstanceMemberTests : BaseMemberTests
{
    [Fact]
    public void Instance_IgnoreNonPublicPropertiesTest()
    {
        TestGeneratedMembersForType<TypeWithNonPublicProperties>();
    }

    [Fact]
    public void Instance_IgnoreFieldsTest()
    {
        TestGeneratedMembersForType<TypeWithFields>();
    }

    [Fact]
    public void Instance_IgnoreMethodsTest()
    {
        TestGeneratedMembersForType<TypeWithMethods>();
    }

    [Fact]
    public void Instance_IncludeReadOnlyProp()
    {
        TestGeneratedMembersForType<TypeWithReadOnlyProperty>();
    }

    [Fact]
    public void Instance_IgnoreSetOnlyProp()
    {
        TestGeneratedMembersForType<TypeWithSetOnlyProperty>();
    }
}

public class BlStaticMemberTests : BaseMemberTests
{
    [Fact]
    public void Static_IgnorePropertiesTest()
    {
        TestGeneratedMembersForType<TypeWithStaticProperies>();
    }

    [Fact]
    public void Static_IgnoreFieldsTest()
    {
        TestGeneratedMembersForType<TypeWithStaticFields>();
    }

    [Fact]
    public void Static_IgnoreConstTest()
    {
        TestGeneratedMembersForType<TypeWithConsts>();
    }

    [Fact]
    public void Static_IgnoreMethodsTest()
    {
        TestGeneratedMembersForType<TypeWithStaticMethods>();
    }

}
