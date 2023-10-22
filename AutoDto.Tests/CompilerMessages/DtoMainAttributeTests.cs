using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.CompilerMessages;

public class DtoMainAttributeTests : BaseCompilerMessageTests
{
    [Fact]
    public void OneDtoRelation_Test()
    {
        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),

            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder(relTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
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
        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))
            .Build(),

            new DtoClassBuilder("RelationNotMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
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
        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            //.AddAttribute(typeof(DtoMainAttribute)) !!
            .Build(),

            new DtoClassBuilder("RelationNotMainDto", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var expected = new MainDtoNotFoundError(BlNamespace + "." + relTypeName, 2);

            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }

    [Fact]
    public void MoreThanOneMainFound_Test()
    {
        var masterTypeName = "MasterType";
        var relTypeName = "RelationType";

        var classes = new List<ClassElement>
        {
            new ClassBuilder(masterTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyBuilder(relTypeName, relTypeName).Build())
            .Build(),

            new ClassBuilder(relTypeName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build(),


            new DtoClassBuilder(masterTypeName + "_Dto", DtoClassBuilder.DtoAttributeType.DtoFrom, masterTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .As<DtoClassBuilder>()
            .SetRelationStrategy(RelationStrategy.ReplaceToDtoProperty)
            .Build(),

            new DtoClassBuilder("RelationMainDto1", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))// !!
            .Build(),

            new DtoClassBuilder("RelationMainDto2", DtoClassBuilder.DtoAttributeType.DtoFrom, relTypeName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .AddAttribute(typeof(DtoMainAttribute))// !!
            .Build(),
        };

        RunWithAssert(classes, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var expected = new MoreThanOneMainDtoFoundError(BlNamespace + "." + relTypeName, 2);

            AssertMessage(DiagnosticSeverity.Error, expected.Id, msgs[0]);
        }
    }
}
