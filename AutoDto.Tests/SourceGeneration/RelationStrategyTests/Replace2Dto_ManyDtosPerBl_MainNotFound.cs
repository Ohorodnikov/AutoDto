using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.SourceGeneration.RelationStrategyTests;

public class Replace2Dto_ManyDtosPerBl_MainNotFound : BaseRelationStrategyTest
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
            //.AddAttribute(typeof(DtoMainAttribute)) !!
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

        RunWithAssert(new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation, dtoForWithRelation }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new MainDtoNotFoundError(classWORelation.Name, 2);

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoForWithRelation.Name)
                    .Skip(1) //skip declaration to get only generated
                    .FirstOrDefault();

            Assert.Null(generatedClass);
        }
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
            //.AddAttribute(typeof(DtoMainAttribute)) !!
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

        RunWithAssert(new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation, dtoForWithRelation }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new MainDtoNotFoundError(classWORelation.Name, 2);

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoForWithRelation.Name)
                    .Skip(1) //skip declaration to get only generated
                    .FirstOrDefault();

            Assert.Null(generatedClass);
        }
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
            //.AddAttribute(typeof(DtoMainAttribute)) !!
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

        RunWithAssert(new[] { classWORelation, classWithRelation, main_dtoForWORelation, secondary_dtoForWORelation, dtoForWithRelation }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new MainDtoNotFoundError(classWORelation.Name, 2);

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoForWithRelation.Name)
                    .Skip(1) //skip declaration to get only generated
                    .FirstOrDefault();

            Assert.Null(generatedClass);
        }
    }
}
