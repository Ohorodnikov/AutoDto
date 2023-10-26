using AutoDto.Tests.SourceGeneration.Models;
using AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;
using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class GeneratedFromBlWithHierarchyTests : BaseUnitTest
{
    [Fact]
    public void BlWithSimpleBaseTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();

        var expectedPropsInDto = new[] 
        { 
            idProp, 
            nameProp 
        };

        var baseBl = 
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace) 
            .AddMember(idProp)
            .Build();

        var blClass =
            new ClassBuilder("SimpleBl")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMember(nameProp)
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, blClass });
    }

    [Fact]
    public void BlIsGenericTypeDefinitionTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var blClass =
            new ClassBuilder("BaseGenericBl<T>")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMember(new PropertyBuilder("Name", "T").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, "BaseGenericBl<string>", blClass.Namespace)
            .SetNamespace(DtoNamespace)
            .Build();

        RunWithAssert(new[] { baseBl, blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(expectedPropsInDto));
        }
    }

    [Fact]
    public void BlHasGenericBaseTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();
        var nameProp2 = new PropertyBuilder("Name2", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp,
            nameProp2,
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var baseBl2 =
            new ClassBuilder("BaseGenericBl<T>")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMember(new PropertyBuilder("Name", "T").Build())
            .Build();

        var blClass =
            new ClassBuilder("GenericBl")
            .SetNamespace(BlNamespace)
            .AddBase("BaseGenericBl<string>")
            .AddMember(nameProp2)
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, baseBl2, blClass });
    }

    [Fact]
    public void BlHasNonPublicPropsTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp,
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var blClass =
            new ClassBuilder("BlWithNonPublicProp")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMember(nameProp)
            .AddMember(new PropertyBuilder("PrivateProp", typeof(string)).SetAccessor(Visibility.Private).Build())
            .AddMember(new PropertyBuilder("ProtectedProp", typeof(string)).SetAccessor(Visibility.Protected).Build())
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, blClass });
    }

    [Fact]
    public void BlHasBaseWithNonPublicPropsTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();
        var nameProp2 = new PropertyBuilder("Name2", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp,
            nameProp2,
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var baseBl2 =
            new ClassBuilder("BlWithNonPublicProp")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMember(nameProp)
            .AddMember(new PropertyBuilder("PrivateProp", typeof(string)).SetAccessor(Visibility.Private).Build())
            .AddMember(new PropertyBuilder("ProtectedProp", typeof(string)).SetAccessor(Visibility.Protected).Build())
            .Build();

        var blClass =
            new ClassBuilder("BlInheritedFromBlWithNonPublicProp")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl2)
            .AddMember(nameProp2)
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, baseBl2, blClass });
    }

    [Fact]
    public void BlHasMembersNotPropsTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp,
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var blClass =
            new ClassBuilder("BlWithMembers")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMembers(new Member[]
            {
                nameProp,

                new MethodBuilder("PrivateMethod", typeof(void)).SetAccessor(Visibility.Private).Build(),
                new MethodBuilder("ProtectedMethod", typeof(void)).SetAccessor(Visibility.Protected).Build(),
                new MethodBuilder("PublicMethod", typeof(void)).SetAccessor(Visibility.Public).Build(),

                new FieldBuilder("PrivateField", typeof(string)).SetAccessor(Visibility.Private).Build(),
                new FieldBuilder("ProtectedField", typeof(string)).SetAccessor(Visibility.Protected).Build(),
                new FieldBuilder("PublicField", typeof(string)).SetAccessor(Visibility.Public).Build(),
            })
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, blClass });
    }

    [Fact]
    public void BlHasBaseWithMembersNotPropsTest()
    {
        var idProp = CommonProperties.Id_Int;
        var nameProp = new PropertyBuilder("Name", typeof(string)).Build();
        var nameProp2 = new PropertyBuilder("Name2", typeof(string)).Build();

        var expectedPropsInDto = new[]
        {
            idProp,
            nameProp,
            nameProp2,
        };

        var baseBl =
            new ClassBuilder("BaseBl")
            .SetNamespace(BlNamespace)
            .AddMember(idProp)
            .Build();

        var baseBl2 =
            new ClassBuilder("BlWithMembers")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl)
            .AddMembers(new Member[]
            {
                nameProp,

                new MethodBuilder("PrivateMethod", typeof(void)).SetAccessor(Visibility.Private).Build(),
                new MethodBuilder("ProtectedMethod", typeof(void)).SetAccessor(Visibility.Protected).Build(),
                new MethodBuilder("PublicMethod", typeof(void)).SetAccessor(Visibility.Public).Build(),

                new FieldBuilder("PrivateField", typeof(string)).SetAccessor(Visibility.Private).Build(),
                new FieldBuilder("ProtectedField", typeof(string)).SetAccessor(Visibility.Protected).Build(),
                new FieldBuilder("PublicField", typeof(string)).SetAccessor(Visibility.Public).Build(),
            })
            .Build();

        var blClass =
            new ClassBuilder("BlInheritedFromBlWithMembers")
            .SetNamespace(BlNamespace)
            .AddBase(baseBl2)
            .AddMember(nameProp2)
            .Build();

        BuildDtoFromLastBlAndCheckAllProps(expectedPropsInDto, new[] { baseBl, baseBl2, blClass });
    }

    private void BuildDtoFromLastBlAndCheckAllProps(PropertyMember[] expectedPropsInDto, ClassElement[] classes)
    {
        var blForDto = classes.Last();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blForDto)
            .SetNamespace(DtoNamespace)
            .Build();

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(expectedPropsInDto));
        }
    }
}
