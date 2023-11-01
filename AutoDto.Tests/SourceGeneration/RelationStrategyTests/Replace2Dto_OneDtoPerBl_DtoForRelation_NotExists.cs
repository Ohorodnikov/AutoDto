using AutoDto.Setup;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration.RelationStrategyTests;

public class Replace2Dto_OneDtoPerBl_DtoForRelation_NotExists : BaseRelationStrategyTest
{
    [Fact]
    public void Relation_Simple_Test()
    {
        var relPropName = "RelationWithoutDto";

        var classWODto =
            new ClassBuilder("TypeWithoutDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithDto =
            new ClassBuilder("TypeWithDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithRelation =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWODto.Name).Build())
            .Build();

        var dtoForWithDto =
            new DtoClassBuilder("TypeWithDto_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithDto)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDescriptor = new TypeDescriptor(classWODto.Namespace, classWODto.Name, TypeType.Simple, null);
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(relTypeDescriptor, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWODto, classWithDto, classWithRelation, dtoForWithDto },
            dtoForWithRelation,
            expectedDtoProps);
    }

    [Fact]
    public void Relation_Array_Test()
    {
        var relPropName = "WithId";

        var classWODto =
            new ClassBuilder("TypeWithoutDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithDto =
            new ClassBuilder("TypeWithDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithRelation =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWODto.Name + "[]").Build())
            .Build();

        var dtoForWithDto =
            new DtoClassBuilder("TypeWithDto_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithDto)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDescriptor = new TypeDescriptor(classWODto.Namespace, classWODto.Name, TypeType.Simple, null);
        var relTypeDescr = new TypeDescriptor(classWODto.Namespace, classWODto.Name + "[]", TypeType.Array, new[] { relTypeDescriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(relTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWODto, classWithDto, classWithRelation, dtoForWithDto },
            dtoForWithRelation,
            expectedDtoProps);
    }

    [Theory]
    [InlineData(nameof(IEnumerable<object>))]
    [InlineData(nameof(List<object>))]
    [InlineData(nameof(HashSet<object>))]
    public void Relation_Collection_Test(string enumerName)
    {
        var _enumerNamespace = typeof(IEnumerable<object>).Namespace;
        var relPropName = "RelationWithoutDto";

        var classWODto =
            new ClassBuilder("TypeWithoutDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithDto =
            new ClassBuilder("TypeWithDto")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var classWithRelation =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddUsing(_enumerNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, $"{enumerName}<{classWODto.Name}>").Build())
            .Build();

        var dtoForWithDto =
            new DtoClassBuilder("TypeWithDto_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithDto)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDescriptor = new TypeDescriptor(classWODto.Namespace, classWODto.Name, TypeType.Simple, null);
        var typeEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { relTypeDescriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeEnumerTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWODto, classWithDto, classWithRelation, dtoForWithDto },
            dtoForWithRelation,
            expectedDtoProps);
    }
}
