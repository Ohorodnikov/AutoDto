using AutoDto.Setup;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration.RelationStrategyTests;

public class SimpleTypeRelationStrategyTests : BaseRelationStrategyTest
{
    [Fact]
    public void Strategy_NotSet_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var dtoClass =
            new ClassBuilder("MyDto")
            .SetNamespace(DtoNamespace)
            .AddUsing(blClass.Namespace)
            .AddAttribute(typeof(DtoFromAttribute), $"typeof({blClass.Name})")
            .SetPartial()
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null), relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Fact]
    public void Strategy_None_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.None)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null), relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Fact]
    public void Strategy_Replace2Id_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.ReplaceToIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeof(int), relPropName + "Id"),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Fact]
    public void Strategy_AddId_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.AddIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeof(int), relPropName + "Id"),
            new PropertyDescriptor(new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null), relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Fact]
    public void Strategy_AddId_FindIdInHierarchy_Test()
    {
        var relPropName = "Relation";

        var baseBl =
            new ClassBuilder("BaseBl<T>")
            .SetNamespace(BlNamespace)
            .AddMember(new PropertyBuilder("Id", "T").Build())
            .Build();

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Name)
            .AddBase("BaseBl<string>")
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelWithHierarchy")
            .SetNamespace(BlNamespace)
            .AddBase("BaseBl<int>")
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.AddIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeof(string), relPropName + "Id"),
            new PropertyDescriptor(new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null), relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { baseBl, classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Test(RelationStrategy strategy)
    {
        var relPropName = "WithoutId";

        var classWOId =
            new ClassBuilder("ClassWithoutId")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelationWithoutId")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWOId.Name).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(strategy)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(new TypeDescriptor(classWOId.Namespace, classWOId.Name, TypeType.Simple, null), relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWOId, blClass }, dtoClass, expectedDtoProps);
    }
}

public class ArrayTypeRelationStrategyTests : BaseRelationStrategyTest
{
    [Fact]
    public void Strategy_Replace2Id_Array_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name + "[]").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.ReplaceToIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeof(int[]), relPropName + "Ids"),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Fact]
    public void Strategy_AddId_Array_Test()
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWORelation.Name + "[]").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.AddIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDescr = new TypeDescriptor(classWORelation.Namespace, classWORelation.Name + "[]", TypeType.Array, new[] { new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null) });

        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeof(int[]), relPropName + "Ids"),
            new PropertyDescriptor(relTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Array_Test(RelationStrategy strategy)
    {
        var relPropName = "WithoutId";

        var classWOId =
            new ClassBuilder("TypeWithoutId")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, classWOId.Name + "[]").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(strategy)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDescr = new TypeDescriptor(classWOId.Namespace, classWOId.Name + "[]", TypeType.Array, new[] { new TypeDescriptor(classWOId.Namespace, classWOId.Name, TypeType.Simple, null) });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(relTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWOId, blClass }, dtoClass, expectedDtoProps);
    }
}

public class EnumerableTypeRelationStrategyTests : BaseRelationStrategyTest
{
    private string _enumerNamespace = typeof(IEnumerable<object>).Namespace;

    [Theory]
    [InlineData(nameof(IEnumerable<object>))]
    [InlineData(nameof(List<object>))]
    [InlineData(nameof(HashSet<object>))]
    public void Strategy_Replace2Id_Collection_Test(string enumerName)
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddUsing(_enumerNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, $"{enumerName}<{classWORelation.Name}>").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.ReplaceToIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var idEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { new TypeDescriptor(typeof(int)) });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(idEnumerTypeDescr, relPropName + "Ids"),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Theory]
    [InlineData(nameof(IEnumerable<object>))]
    [InlineData(nameof(List<object>))]
    [InlineData(nameof(HashSet<object>))]
    public void Strategy_AddId_Collection_Test(string enumerName)
    {
        var relPropName = "WithId";

        var classWORelation =
            new ClassBuilder("TypeWithoutRelation")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddUsing(_enumerNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, $"{enumerName}<{classWORelation.Name}>").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(RelationStrategy.AddIdProperty)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDecriptor = new TypeDescriptor(classWORelation.Namespace, classWORelation.Name, TypeType.Simple, null);
        var intTypeDescriptor = new TypeDescriptor(typeof(int));
        var idEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { intTypeDescriptor });
        var typeEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { relTypeDecriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(idEnumerTypeDescr, relPropName + "Ids"),
            new PropertyDescriptor(typeEnumerTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWORelation, blClass }, dtoClass, expectedDtoProps);
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Collection_Test(RelationStrategy strategy)
    {
        var enumerName = nameof(IEnumerable<object>);
        var relPropName = "WithoutId";

        var classWOId =
            new ClassBuilder("TypeWithoutId")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .Build();

        var blClass =
            new ClassBuilder("TypeWithRelation")
            .SetNamespace(BlNamespace)
            .AddUsing(_enumerNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(CommonProperties.Description)
            .AddMember(new PropertyBuilder(relPropName, $"{enumerName}<{classWOId.Name}>").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetRelationStrategy(strategy)
            .SetNamespace(DtoNamespace)
            .Build();

        var relTypeDecriptor = new TypeDescriptor(classWOId.Namespace, classWOId.Name, TypeType.Simple, null);
        var typeEnumerTypeDescr = new TypeDescriptor(_enumerNamespace, enumerName + "`1", TypeType.Generic, new[] { relTypeDecriptor });
        var expectedDtoProps = new PropertyDescriptor[]
        {
            new PropertyDescriptor(typeof(int), "Id"),
            new PropertyDescriptor(typeof(string), "Name"),
            new PropertyDescriptor(typeof(string), "Description"),
            new PropertyDescriptor(typeEnumerTypeDescr, relPropName),
        };

        TestGeneratedDtoForExpectedProps(new[] { classWOId, blClass }, dtoClass, expectedDtoProps);
    }
}
