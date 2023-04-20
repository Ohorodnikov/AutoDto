using AutoDto.Setup;
using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class OneTypeRelationStrategyTests : BaseUnitTest
{
    #region relation object
    [Fact]
    public void Strategy_NotSet_Test()
    {
        var blType = typeof(TypeWithRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelation.Id)),
            blType.GetProperty(nameof(TypeWithRelation.Name)),
            blType.GetProperty(nameof(TypeWithRelation.Description)),
            blType.GetProperty(nameof(TypeWithRelation.WithId)),
        }
        .Select(x => new PropertyDescriptor(x))
        .ToList();

        var type = typeof(TypeWithRelation);
        var attr = typeof(DtoFromAttribute);
        var name = attr.Name.Replace(nameof(Attribute), "");

        var code = $@"
using {attr.Namespace};
using {type.Namespace};

namespace AutoDto.Tests.Dto;

[{name}(typeof({type.Name}))]
{DtoCreator.GetPublicDtoDef("MyDto")} {{ }}
";

        var compilation = Generator.Run(code);

        var genClass = SyntaxChecker.FindClassByName(compilation, "MyDto");

        SyntaxChecker.TestOneClassDeclaration(genClass, expected);        
    }

    [Fact]
    public void Strategy_None_Test()
    {
        var blType = typeof(TypeWithRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelation.Id)),
            blType.GetProperty(nameof(TypeWithRelation.Name)),
            blType.GetProperty(nameof(TypeWithRelation.Description)),
            blType.GetProperty(nameof(TypeWithRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        RunTestWithOneType(blType, RelationStrategy.None, expected.ToArray());
    }

    [Fact]
    public void Strategy_Replace2Id_Test() 
    {
        var blType = typeof(TypeWithRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelation.Id)),
            blType.GetProperty(nameof(TypeWithRelation.Name)),
            blType.GetProperty(nameof(TypeWithRelation.Description)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((typeof(long), nameof(TypeWithRelation.WithId) + "Id"));

        RunTestWithOneType(blType, RelationStrategy.ReplaceToIdProperty, expected.ToArray());
    }

    [Fact]
    public void Strategy_AddId_Test()
    {
        var blType = typeof(TypeWithRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelation.Id)),
            blType.GetProperty(nameof(TypeWithRelation.Name)),
            blType.GetProperty(nameof(TypeWithRelation.Description)),
            blType.GetProperty(nameof(TypeWithRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((typeof(long), nameof(TypeWithRelation.WithId) + "Id"));

        RunTestWithOneType(blType, RelationStrategy.AddIdProperty, expected.ToArray());
    }

    [Fact]
    public void Strategy_AddId_FindIdInHierarchy_Test()
    {
        var blType = typeof(TypeWithRelWithHierarchy);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelWithHierarchy.Id)),
            blType.GetProperty(nameof(TypeWithRelWithHierarchy.Description)),
            blType.GetProperty(nameof(TypeWithRelWithHierarchy.Relation)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((typeof(string), nameof(TypeWithRelWithHierarchy.Relation) + "Id"));

        RunTestWithOneType(blType, RelationStrategy.AddIdProperty, expected.ToArray());
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Test(RelationStrategy strategy)
    {
        var blType = typeof(TypeWithRelationWithoutId);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithRelationWithoutId.Id)),
            blType.GetProperty(nameof(TypeWithRelationWithoutId.Name)),
            blType.GetProperty(nameof(TypeWithRelationWithoutId.Description)),
            blType.GetProperty(nameof(TypeWithRelationWithoutId.WithoutId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        RunTestWithOneType(blType, strategy, expected.ToArray());
    }

    #endregion
    #region relation collection

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation), typeof(IEnumerable<long>))]
    [InlineData(typeof(TypeWithListRelation), typeof(List<long>))]
    [InlineData(typeof(TypeWithHashSetRelation), typeof(HashSet<long>))]
    public void Strategy_Replace2Id_Collection_Test(Type blType, Type propType)
    {
        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Id)),
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Name)),
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Description)),
            //blType.GetProperty(nameof(TypeWithEnumerableRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((propType, nameof(TypeWithEnumerableRelation.WithId) + "Ids"));

        RunTestWithOneType(blType, RelationStrategy.ReplaceToIdProperty, expected.ToArray());
    }

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation), typeof(IEnumerable<long>))]
    [InlineData(typeof(TypeWithListRelation), typeof(List<long>))]
    [InlineData(typeof(TypeWithHashSetRelation), typeof(HashSet<long>))]
    public void Stategy_AddId_Collection_Test(Type blType, Type propType)
    {
        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Id)),
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Name)),
            blType.GetProperty(nameof(TypeWithEnumerableRelation.Description)),
            blType.GetProperty(nameof(TypeWithEnumerableRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((propType, nameof(TypeWithEnumerableRelation.WithId) + "Ids"));

        RunTestWithOneType(blType, RelationStrategy.AddIdProperty, expected.ToArray());
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Collection_Test(RelationStrategy strategy)
    {
        var blType = typeof(TypeWithEnumerableRelationWithoutId);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithEnumerableRelationWithoutId.Id)),
            blType.GetProperty(nameof(TypeWithEnumerableRelationWithoutId.Name)),
            blType.GetProperty(nameof(TypeWithEnumerableRelationWithoutId.Description)),
            blType.GetProperty(nameof(TypeWithEnumerableRelationWithoutId.WithoutId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        RunTestWithOneType(blType, strategy, expected.ToArray());
    }

    #endregion
    #region relation array

    [Fact]
    public void Strategy_Replace2Id_Array_Test()
    {
        var blType = typeof(TypeWithArrayRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithArrayRelation.Id)),
            blType.GetProperty(nameof(TypeWithArrayRelation.Name)),
            blType.GetProperty(nameof(TypeWithArrayRelation.Description)),
            //blType.GetProperty(nameof(TypeWithArrayRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((typeof(long[]), nameof(TypeWithArrayRelation.WithId) + "Ids"));

        RunTestWithOneType(blType, RelationStrategy.ReplaceToIdProperty, expected.ToArray());
    }

    [Fact]
    public void Stategy_AddId_Array_Test()
    {
        var blType = typeof(TypeWithArrayRelation);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithArrayRelation.Id)),
            blType.GetProperty(nameof(TypeWithArrayRelation.Name)),
            blType.GetProperty(nameof(TypeWithArrayRelation.Description)),
            blType.GetProperty(nameof(TypeWithArrayRelation.WithId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        expected.Add((typeof(long[]), nameof(TypeWithEnumerableRelation.WithId) + "Ids"));

        RunTestWithOneType(blType, RelationStrategy.AddIdProperty, expected.ToArray());
    }

    [Theory]
    [InlineData(RelationStrategy.None)]
    [InlineData(RelationStrategy.ReplaceToIdProperty)]
    [InlineData(RelationStrategy.AddIdProperty)]
    public void RelationWithoutId_Array_Test(RelationStrategy strategy)
    {
        var blType = typeof(TypeWithArrayRelationWithoutId);

        var expected = new[]
        {
            blType.GetProperty(nameof(TypeWithArrayRelationWithoutId.Id)),
            blType.GetProperty(nameof(TypeWithArrayRelationWithoutId.Name)),
            blType.GetProperty(nameof(TypeWithArrayRelationWithoutId.Description)),
            blType.GetProperty(nameof(TypeWithArrayRelationWithoutId.WithoutId)),
        }
        .Select(x => (x.PropertyType, x.Name))
        .ToList();

        RunTestWithOneType(blType, strategy, expected.ToArray());
    }

    #endregion

    private void RunTestWithOneType(Type type, RelationStrategy strategy, (Type type, string name)[] expectedProps)
    {
        var dto = new DtoData(type, strategy, "MyDto");

        var compilation = RunForDtos(dto);

        var genClass = SyntaxChecker.FindClassByName(compilation, dto.DtoName);

        var exp = expectedProps.Select(x => new PropertyDescriptor(new TypeDescriptor(x.type), x.name)); 

        SyntaxChecker.TestOneClassDeclaration(genClass, exp);
    }
}
