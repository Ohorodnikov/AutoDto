using AutoDto.Setup;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration.RelationStrategyTests;

public class Replace2Dto_ManyDtosPerBl_OneMainAttribute : BaseRelationStrategyTest
{
    [Fact]
    public void Relation_Simple_Test()
    {
        var relPropName = "Relation";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
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
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var main_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_Main", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))
            .Build();

        var secondary_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_2", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relDtoTypeDescriptor = new TypeDescriptor(main_dtoForWORelation.Namespace, main_dtoForWORelation.Name, TypeType.Simple, null);
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(relDtoTypeDescriptor, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation },
            dtoForWithRelation,
            expectedDtoProps);
    }

    [Fact]
    public void Relation_Array_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
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
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name + "[]").Build())
            .Build();

        var main_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_Main", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))
            .Build();

        var secondary_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_2", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relDtoTypeDescriptor = new TypeDescriptor(main_dtoForWORelation.Namespace, main_dtoForWORelation.Name, TypeType.Simple, null);
        var relTypeDescr = new TypeDescriptor(main_dtoForWORelation.Namespace, main_dtoForWORelation.Name + "[]", TypeType.Array, new[] { relDtoTypeDescriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(relTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation },
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
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
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
            .AddMember(new PropertyBuilder(relPropName, $"{enumerName}<{classWORelation.Name}>").Build())
            .Build();

        var main_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_Main", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))
            .Build();

        var secondary_dtoForWORelation =
            new DtoClassBuilder("TypeWithoutRelation_Dto_2", DtoClassBuilder.DtoAttributeType.DtoFrom, classWORelation)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var dtoForWithRelation =
            new DtoClassBuilder("TypeWithRelation_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, classWithRelation)
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relDtoTypeDescriptor = new TypeDescriptor(main_dtoForWORelation.Namespace, main_dtoForWORelation.Name, TypeType.Simple, null);
        var typeEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { relDtoTypeDescriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeEnumerTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(
            new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation },
            dtoForWithRelation,
            expectedDtoProps);
    }
}
