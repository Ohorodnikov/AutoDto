using AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;
using AutoDto.Tests.TestHelpers;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class GeneratedFromBlWithHierarchyTests : BaseUnitTest
{
    private static string _dtoName = "MyDto";

    private DtoData NewData(Type blType)
    {
        return new DtoData(blType, Setup.RelationStrategy.None, _dtoName);
    }

    [Theory]
    [InlineData(typeof(SimpleBl))]
    [InlineData(typeof(BaseGenericBl<string>))]
    [InlineData(typeof(GenericBl))]
    [InlineData(typeof(BlWithNonPublicProp))]
    [InlineData(typeof(BlInheritedFromBlWithNonPublicProp))]
    [InlineData(typeof(BlWithMembers))]
    [InlineData(typeof(BlInheritedFromBlWithMembers))]
    public void BlWithHierarchy_IncludePublicProps_Test(Type blType)
    {
        var dtoData = NewData(blType);

        var compilation = RunForDtos(dtoData);

        var generated = SyntaxChecker.FindClassByName(compilation, dtoData.DtoName);

        var expected = dtoData.Type.GetPublicInstProperties().Select(x => new PropertyDescriptor(x));

        SyntaxChecker.TestOneClassDeclaration(generated, expected);
    }
}
