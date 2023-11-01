using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration.RelationStrategyTests;

public class BaseRelationStrategyTest : BaseUnitTest
{
    protected void TestGeneratedDtoForExpectedProps(IEnumerable<ClassElement> classes, ClassElement dtoClass2Check, PropertyDescriptor[] expectedPropsInDto)
    {
        RunWithAssert(classes.Append(dtoClass2Check), (compilation, msgs) =>
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass2Check.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, expectedPropsInDto);
        });
    }
}
