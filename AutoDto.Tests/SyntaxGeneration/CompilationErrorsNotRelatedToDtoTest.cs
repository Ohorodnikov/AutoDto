using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.SyntaxGeneration;

public class CompilationErrorsNotRelatedToDtoTest : BaseCompilationErrorTests
{
    private ClassElement GetValidDtoForBl(string blName, string dtoName = "MyDto")
    {
        return
            new DtoClassBuilder(dtoName, DtoClassBuilder.DtoAttributeType.DtoFrom, blName)
            .SetNamespace(DtoNamespace)
            .AddUsing(BlNamespace)
            .Build();
    }

    private ClassElement GetValidBl(string blName)
    {
        return new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build();
    }

    private ClassElement GetClassWithCompilationError()
    {
        return
            new ClassBuilder("ClassWithError")
            .SetNamespace("SomeNamespace")
            .AddMember(new MethodMember("public voi SomeMethod() { }"))
            .Build();
    }

    [Fact]
    public void CompilationErrorInFileWithOneDto()
    {
        var bl = GetValidBl("BlClass");
        var dto = GetValidDtoForBl(bl.Name);
        var classWithErr = GetClassWithCompilationError();

        var dtoFileContent = dto.GenerateCode() + Environment.NewLine + classWithErr.GenerateCode();

        AssertGeneratorWasRunNTimes(1, 1, bl.GenerateCode(), dtoFileContent);
    }

    [Fact]
    public void CompilationErrorInFileWithManyDto()
    {
        var bl1 = GetValidBl("Bl1");
        var bl2 = GetValidBl("Bl2");

        var dto1 = GetValidDtoForBl(bl1.Name, "Dto1");
        var dto2 = GetValidDtoForBl(bl2.Name, "Dto2");
        var classWithErr = GetClassWithCompilationError();

        var dtoFileContent =
            dto1.GenerateCode() +
            Environment.NewLine +
            dto2.GenerateCode() +
            Environment.NewLine +
            classWithErr.GenerateCode()
            ;

        AssertGeneratorWasRunNTimes(1, 2, bl1.GenerateCode(), bl2.GenerateCode(), dtoFileContent);
    }

    [Fact]
    public void CompilationErrorInFileWithBl()
    {
        var bl = GetValidBl("BlClass");
        var dto = GetValidDtoForBl(bl.Name);
        var classWithErr = GetClassWithCompilationError();

        var blFileContent = bl.GenerateCode() + Environment.NewLine + classWithErr.GenerateCode();

        AssertGeneratorWasRunNTimes(1, 1, blFileContent, dto.GenerateCode());
    }

    [Fact]
    public void CompilationErrorInFileNotRelatedToDto()
    {
        var bl = GetValidBl("BlClass");
        var dto = GetValidDtoForBl(bl.Name);
        var classWithErr = GetClassWithCompilationError();

        AssertGeneratorWasRunNTimes(1, 1, bl.GenerateCode(), dto.GenerateCode(), classWithErr.GenerateCode());
    }
}
