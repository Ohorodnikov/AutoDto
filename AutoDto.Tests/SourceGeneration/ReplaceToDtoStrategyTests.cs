using AutoDto.Setup;
using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class ReplaceToDtoStrategyTests : BaseUnitTest
{
    private List<(string, string)> GetMainAttr()
    {
        var attr = typeof(DtoMainAttribute);

        return new List<(string, string)>
        {
            (attr.Namespace, $"[{attr.Name}]"),
        };
    }

    private void RunOneRelation(DtoData[] dtos, string relationPropName, TypeDescriptor newPropType)
    {
        var masterDef = dtos[0];

        var compilation = RunForDtos(dtos);

        Assert.Equal(1 + dtos.Length, compilation.SyntaxTrees.Count());

        var masterDto = SyntaxChecker.FindClassByName(compilation, masterDef.DtoName);

        var expectedProps = masterDef.Type
                                    .GetProperties()
                                    .Where(x => x.Name != relationPropName)
                                    .Select(x => new PropertyDescriptor(x))
                                    .ToList();

        expectedProps.Add(new PropertyDescriptor(newPropType, relationPropName));

        SyntaxChecker.TestOneClassDeclaration(masterDto, expectedProps);
    }

    #region one dto relation

    [Fact]
    public void RelationDto_OneDto_Found_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationDto");

        var newPropType = new TypeDescriptor(DtoTypNamespace, relDef.DtoName, TypeType.Simple, null);

        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithRelation.WithId), newPropType);
    }

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation), typeof(IEnumerable<long>))]
    [InlineData(typeof(TypeWithListRelation), typeof(List<long>))]
    [InlineData(typeof(TypeWithHashSetRelation), typeof(HashSet<long>))]
    public void RelationDto_OneDto_Found_Collection_Test(Type blType, Type propType)
    {
        var masterDef = new DtoData(blType, RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationDto");

        var newPropTypeParam = new TypeDescriptor(DtoTypNamespace, relDef.DtoName, TypeType.Simple, null);

        var newPropType = new TypeDescriptor(propType.Namespace, propType.Name, TypeType.Generic, new[] {newPropTypeParam} );

        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithEnumerableRelation.WithId), newPropType);
    }

    [Fact]
    public void RelationDto_OneDto_Found_Array_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithArrayRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationDto");

        var newPropTypeParam = new TypeDescriptor(DtoTypNamespace, relDef.DtoName, TypeType.Simple, null);

        var enumerType = typeof(TypeWithoutRelation[]);

        var newPropType = new TypeDescriptor(enumerType.Namespace, enumerType.Name, TypeType.Array, new[] { newPropTypeParam });

        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithArrayRelation.WithId), newPropType);
    }

    [Fact]
    public void RelationDto_OneDto_NotFound_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutId), RelationStrategy.None, "SomeDto");

        var newPropType = new TypeDescriptor(masterDef.Type.GetProperty(nameof(TypeWithRelation.WithId)).PropertyType);

        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithRelation.WithId), newPropType);
    }

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation))]
    [InlineData(typeof(TypeWithListRelation))]
    [InlineData(typeof(TypeWithHashSetRelation))]
    public void RelationDto_OneDto_NotFound_Collection_Test(Type blType)
    {
        var masterDef = new DtoData(blType, RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutId), RelationStrategy.None, "SomeDto");

        var newPropType = new TypeDescriptor(masterDef.Type.GetProperty(nameof(TypeWithEnumerableRelation.WithId)).PropertyType);
        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithEnumerableRelation.WithId), newPropType);
    }

    [Fact]
    public void RelationDto_OneDto_NotFound_Array_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithArrayRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDef = new DtoData(typeof(TypeWithoutId), RelationStrategy.None, "SomeDto");

        var newPropType = new TypeDescriptor(masterDef.Type.GetProperty(nameof(TypeWithArrayRelation.WithId)).PropertyType);
        RunOneRelation(new[] { masterDef, relDef }, nameof(TypeWithArrayRelation.WithId), newPropType);
    }

    #endregion

    #region many dtos for relationDto

    #region one main    

    [Fact]
    public void RelationDto_ManyDtos_HasOneMain_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        var relationPropName = nameof(TypeWithRelation.WithId);

        var newPropType = new TypeDescriptor(DtoTypNamespace, relDefMain.DtoName, TypeType.Simple, null);

        RunOneRelation(new[] {masterDef, relDefMain, relDefNotMain}, relationPropName, newPropType);
    }

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation), typeof(IEnumerable<long>))]
    [InlineData(typeof(TypeWithListRelation), typeof(List<long>))]
    [InlineData(typeof(TypeWithHashSetRelation), typeof(HashSet<long>))]
    public void RelationDto_ManyDtos_HasOneMain_Collection_Test(Type blType, Type propType)
    {
        var masterDef = new DtoData(blType, RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        var newPropTypeParam = new TypeDescriptor(DtoTypNamespace, relDefMain.DtoName, TypeType.Simple, null);

        var newPropType = new TypeDescriptor(propType.Namespace, propType.Name, TypeType.Generic, new[] { newPropTypeParam });

        RunOneRelation(new[] { masterDef, relDefMain, relDefNotMain }, nameof(TypeWithEnumerableRelation.WithId), newPropType);
    }
    [Fact]
    public void RelationDto_ManyDtos_HasOneMain_Array_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithArrayRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        var newPropTypeParam = new TypeDescriptor(DtoTypNamespace, relDefMain.DtoName, TypeType.Simple, null);

        var enumerType = typeof(TypeWithoutRelation[]);

        var newPropType = new TypeDescriptor(enumerType.Namespace, enumerType.Name, TypeType.Array, new[] { newPropTypeParam });

        RunOneRelation(new[] { masterDef, relDefMain, relDefNotMain }, nameof(TypeWithArrayRelation.WithId), newPropType);
    }
    #endregion

    #region main not found

    private void AssertNoMasterDto(DtoData[] dtos)
    {
        var masterDto = dtos[0];

        var compilation = RunForDtos(dtos);

        var masterDtoGenerated = SyntaxChecker.FindClassByName(compilation, masterDto.DtoName);

        Assert.Null(masterDtoGenerated);
    }

    [Fact]
    public void RelationDto_ManyDtos_MainNotFound_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto");
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }

    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation))]
    [InlineData(typeof(TypeWithListRelation))]
    [InlineData(typeof(TypeWithHashSetRelation))]
    public void RelationDto_ManyDtos_MainNotFound_Collection_Test(Type blType)
    {
        var masterDef = new DtoData(blType, RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto");
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }

    [Fact]
    public void RelationDto_ManyDtos_MainNotFound_Array_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithArrayRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto");
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto");

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }
    #endregion

    #region many mains
    
    [Fact]
    public void RelationDto_ManyDtos_HasManyMain_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto", GetMainAttr());

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }
    
    [Theory]
    [InlineData(typeof(TypeWithEnumerableRelation))]
    [InlineData(typeof(TypeWithListRelation))]
    [InlineData(typeof(TypeWithHashSetRelation))]
    public void RelationDto_ManyDtos_HasManyMain_Collection_Test(Type blType)
    {
        var masterDef = new DtoData(blType, RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto", GetMainAttr());

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }
    
    [Fact]
    public void RelationDto_ManyDtos_HasManyMain_Array_Test()
    {
        var masterDef = new DtoData(typeof(TypeWithArrayRelation), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(TypeWithoutRelation), RelationStrategy.None, "RelationNotMainDto", GetMainAttr());

        AssertNoMasterDto(new[] { masterDef, relDefMain, relDefNotMain });
    }
    #endregion
    
    #endregion
}
