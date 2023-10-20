using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.Tests.CompilerMessages.Models;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
//using AutoDto.Tests.SourceGeneration.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
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
        var blNamespace = "AutoDto.Tests.CompilerMessages.Models";
        var dtoNamespace = "AutoDto.Tests.CompilerMessages.Dtos";

        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>()
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),

            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder(relTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);
        }
    }

    [Fact]
    public void MainFound_Test()
    {
        var blNamespace = "AutoDto.Tests.CompilerMessages.Models";
        var dtoNamespace = "AutoDto.Tests.CompilerMessages.Dtos";

        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>()
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))
            .Build(),

            new DtoClassBuilder("RelationNotMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);
        }
    }

    [Fact]
    public void MainNotFound_Test()
    {
        var blNamespace = "AutoDto.Tests.CompilerMessages.Models";
        var dtoNamespace = "AutoDto.Tests.CompilerMessages.Dtos";

        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>()
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            //.AddAttribute(typeof(DtoMainAttribute)) !!
            .Build(),

            new DtoClassBuilder("RelationNotMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var expected = new MainDtoNotFoundError(blNamespace + "." + relTypeName, 2);

            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void MoreThanOneMainFound_Test()
    {
        var blNamespace = "AutoDto.Tests.CompilerMessages.Models";
        var dtoNamespace = "AutoDto.Tests.CompilerMessages.Dtos";

        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>()
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(blNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto1", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))// !!
            .Build(),

            new DtoClassBuilder("RelationMainDto2", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, blNamespace)
            .SetNamespace(dtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))// !!
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var expected = new MoreThanOneMainDtoFoundError(blNamespace + "." + relTypeName, 2);

            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }
}
