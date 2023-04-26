using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.Tests.CompilerMessages.Models;
//using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;

namespace AutoDto.Tests.CompilerMessages;

public class DtoMainAttributeTests : BaseCompilerMessageTests
{
    private List<(string, string)> GetMainAttr()
    {
        var attr = typeof(DtoMainAttribute);

        return new List<(string, string)>
        {
            (attr.Namespace, $"[{attr.Name}]"),
        };
    }

    [Fact]
    public void OneDtoRelation_Test()
    {
        var masterDef = new DtoData(typeof(MasterType), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationMainDto");

        var dtos = new[] { masterDef, relDefMain };

        var code = DtoCreator.GetDtosDefinition(dtos);

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Empty(msgs);
    }

    [Fact]
    public void MainFound_Test()
    {
        var masterDef = new DtoData(typeof(MasterType), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationNotMainDto");

        var dtos = new[] { masterDef, relDefMain, relDefNotMain };

        var code = DtoCreator.GetDtosDefinition(dtos);

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Empty(msgs);
    }

    [Fact]
    public void MainNotFound_Test()
    {
        var masterDef = new DtoData(typeof(MasterType), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationMainDto");
        var relDefNotMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationNotMainDto");

        var dtos = new[] { masterDef, relDefMain, relDefNotMain };

        var code = DtoCreator.GetDtosDefinition(dtos);

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(msgs);

        var expected = new MainDtoNotFoundError(relDefMain.Type.FullName, 2);

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }

    [Fact]
    public void MoreThanOneMainFound_Test()
    {
        var masterDef = new DtoData(typeof(MasterType), RelationStrategy.ReplaceToDtoProperty, "MasterDto");
        var relDefMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationMainDto", GetMainAttr());
        var relDefNotMain = new DtoData(typeof(RelationType), RelationStrategy.None, "RelationNotMainDto", GetMainAttr());

        var dtos = new[] { masterDef, relDefMain, relDefNotMain };

        var code = DtoCreator.GetDtosDefinition(dtos);

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(msgs);

        var expected = new MoreThanOneMainDtoFoundError(relDefMain.Type.FullName, 2);

        AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
    }
}
