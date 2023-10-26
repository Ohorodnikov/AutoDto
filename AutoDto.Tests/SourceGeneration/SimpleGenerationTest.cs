using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.SourceGeneration;

public class SimplePropGenerationTests : BaseUnitTest
{
    [Fact]
    public void IntPropertyTest()
    {
        GenerateBlWithPropsAndThemInDto(CommonProperties.Id_Int);
    }

    [Fact]
    public void StringPropertyTest()
    {
        GenerateBlWithPropsAndThemInDto(CommonProperties.Name);
    }

    [Fact]
    public void DateTimePropertyTest()
    {
        GenerateBlWithPropsAndThemInDto(new PropertyBuilder("DateTimeProp", typeof(DateTime)).Build());
    }

    [Fact]
    public void EnumPropertyTest()
    {
        GenerateBlWithPropsAndThemInDto(new PropertyBuilder("EnumProp", typeof(DateTimeKind)).Build());
    }

    [Fact]
    public void RefTypePropertyTest()
    {
        GenerateBlWithPropsAndThemInDto(new PropertyBuilder("RefProp", typeof(object)).Build());
    }

    [Fact]
    public void MultiplePropsTest()
    {
        GenerateBlWithPropsAndThemInDto(new[]
        {
            CommonProperties.Id_Int,
            CommonProperties.Name,
            new PropertyBuilder("Kind", typeof(DateTimeKind)).Build(),
            new PropertyBuilder("RefTypeProp", typeof(object)).Build(),
        });
    }

    private void GenerateBlWithPropsAndThemInDto(params PropertyMember[] props)
    {
        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(props)
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoType", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(props));
        }
    }
}
