using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class BaseMemberTests : BaseUnitTest
{
    protected void GenerateBlAndAssertDtoMembers(Member[] blMembers, IEnumerable<PropertyDescriptor> expectedDtoProperties)
    {
        var blClass = new ClassBuilder("MemberTestBl")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder("MemberTestsDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass.Name, blClass.Namespace)
            .SetNamespace(DtoNamespace)
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                .Skip(1) //skip declaration to get only generated
                .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, expectedDtoProperties);
        }
    }
}

public class BlInstanceMemberTests : BaseMemberTests
{
    [Fact]
    public void Instance_IgnoreNonPublicPropertiesTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new PropertyBuilder("PrivateProp", typeof(string)).SetAccessor(Visibility.Private).Build(),
            new PropertyBuilder("ProtectedProp", typeof(string)).SetAccessor(Visibility.Protected).Build(),
            new PropertyBuilder("InternalProp", typeof(string)).SetAccessor(Visibility.Internal).Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Instance_IgnoreFieldsTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new FieldBuilder("_privateField", typeof(string)).SetAccessor(Visibility.Private).Build(),
            new FieldBuilder("_protectedField", typeof(string)).SetAccessor(Visibility.Protected).Build(),
            new FieldBuilder("_internalField", typeof(string)).SetAccessor(Visibility.Internal).Build(),
            new FieldBuilder("_publicField", typeof(string)).SetAccessor(Visibility.Public).Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Instance_IgnoreMethodsTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new MethodBuilder("PrivateMethod", typeof(string)).SetAccessor(Visibility.Private).Build(),
            new MethodBuilder("ProtectedMethod", typeof(string)).SetAccessor(Visibility.Protected).Build(),
            new MethodBuilder("InternalMethod", typeof(string)).SetAccessor(Visibility.Internal).Build(),
            new MethodBuilder("PublicMethod", typeof(string)).SetAccessor(Visibility.Public).Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Instance_IncludeReadOnlyProp()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,
            new PropertyBuilder("ReadOnlyProp", typeof(string)).DefineSet(false).Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "ReadOnlyProp")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Instance_IgnoreSetOnlyProp()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,
            new PropertyBuilder("PrivateReadPublicSetProp", typeof(string)).DefineGet(Visibility.Private).Build(),
            new PropertyMember("public string SetOnlyProp { set { }}")
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }
}

public class BlStaticMemberTests : BaseMemberTests
{
    [Fact]
    public void Static_IgnorePropertiesTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new PropertyBuilder("PrivateProp", typeof(string)).SetAccessor(Visibility.Private).SetStatic().Build(),
            new PropertyBuilder("ProtectedProp", typeof(string)).SetAccessor(Visibility.Protected).SetStatic().Build(),
            new PropertyBuilder("InternalProp", typeof(string)).SetAccessor(Visibility.Internal).SetStatic().Build(),
            new PropertyBuilder("PublicProp", typeof(string)).SetAccessor(Visibility.Public).SetStatic().Build(),
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Static_IgnoreFieldsTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new FieldBuilder("_privateField", typeof(string)).SetAccessor(Visibility.Private).SetStatic().Build(),
            new FieldBuilder("_protectedField", typeof(string)).SetAccessor(Visibility.Protected).SetStatic().Build(),
            new FieldBuilder("_internalField", typeof(string)).SetAccessor(Visibility.Internal).SetStatic().Build(),
            new FieldBuilder("_publicField", typeof(string)).SetAccessor(Visibility.Public).SetStatic().Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Static_IgnoreConstTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new ConstantBuilder("PRIVATE_CONST", typeof(string), "\"\"").SetAccessor(Visibility.Private).Build(),
            new ConstantBuilder("PROTECTED_CONST", typeof(string), "\"\"").SetAccessor(Visibility.Protected).Build(),
            new ConstantBuilder("INTERNAL_CONST", typeof(string), "\"\"").SetAccessor(Visibility.Internal).Build(),
            new ConstantBuilder("PUBLIC_CONST", typeof(string), "\"\"").SetAccessor(Visibility.Public).Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }

    [Fact]
    public void Static_IgnoreMethodsTest()
    {
        var blMembers = new Member[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,

            new MethodBuilder("PrivateMethod", typeof(string)).SetAccessor(Visibility.Private).SetStatic().Build(),
            new MethodBuilder("ProtectedMethod", typeof(string)).SetAccessor(Visibility.Protected).SetStatic().Build(),
            new MethodBuilder("InternalMethod", typeof(string)).SetAccessor(Visibility.Internal).SetStatic().Build(),
            new MethodBuilder("PublicMethod", typeof(string)).SetAccessor(Visibility.Public).SetStatic().Build()
        };

        var expectedDtoMembers = new[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name")
        };

        GenerateBlAndAssertDtoMembers(blMembers, expectedDtoMembers);
    }
}
