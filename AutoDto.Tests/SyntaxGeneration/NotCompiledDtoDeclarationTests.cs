using AutoDto.Setup;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledDtoDeclarationTests : BaseCompilationErrorTests
{
    private ClassElement GetValidBl(string blName = "MyBl")
    {
        return
            new ClassBuilder(blName)
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build();
    }

    [Fact]
    public void BrokenOneOfPartialClassesTest()
    {
        var bl = GetValidBl();

        var dto1 =
            new ClassBuilder("MyDto")
            .SetNamespace(DtoNamespace)
            .AddUsing(bl.Namespace)
            .SetPartial()
            .AddAttribute(typeof(DtoFromAttribute), $"typef({bl.Name})")
            .Build();

        var dto2 =
            new DtoClassBuilder(dto1.Name, DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyMember("publ int Id { get; set; }"))
            .Build();

        AssertGeneratorWasNotRun(dto1.GenerateCode(), dto2.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenAttributeDefinitionTest()
    {
        var bl = GetValidBl();

        var dto =
            new ClassBuilder("MyDto")
            .SetNamespace(DtoNamespace)
            .AddUsing(bl.Namespace)
            .SetPartial()
            .AddAttribute(typeof(DtoFromAttribute), $"typef({bl.Name})")
            .Build();

        AssertGeneratorWasNotRun(bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void BrokenKeywordsTest()
    {
        var bl = GetValidBl();

        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};
using {bl.Namespace};

namespace {DtoNamespace};

[DtoFrom(typeof({bl.Name}))]
puic partial class MyDto
{{ }}
";

        AssertGeneratorWasNotRun(code, bl.GenerateCode());
    }

    [Fact]
    public void BrokenOwnPropertyTest()
    {
        var bl = GetValidBl();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyMember("public strin MyOwnProperty { get; set; }"))
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenOwnProperty2()
    {
        var bl = GetValidBl();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyMember("public string MyOwnProperty { get; se }"))
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenMethodDefinitionTest()
    {
        var bl = GetValidBl();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(new MethodMember("public void MyMethod(bool param1, ) { }"))
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void PragmaErrorTest()
    {
        var bl = GetValidBl();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(new FieldMember("#error MyTestError")) //pragma instead of field
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenMethodContentTest()
    {
        var bl = GetValidBl();

        var brokenMethod =
            new MethodBuilder("MyMethod", typeof(void))
            .AddArgument("bool", "param1")
            .SetBody("var q = param1.Call();")
            .Build();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddMember(brokenMethod)
            .Build();

        AssertGeneratorWasRunNTimes(1, 1, dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenInheritanceDeclarationTest()
    {
        var bl = GetValidBl();

        var baseDto = new ClassBuilder("BaseDto")
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyBuilder("BaseDtoProp", typeof(string)).Build())
            .Build();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDto)
            .AddBase(" ")
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }

    [Fact]
    public void BrokenBaseDtoTest()
    {
        var bl = GetValidBl();

        var baseDto = new ClassBuilder("BaseDto")
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyMember("public str BaseDtoProp { get; set; }"))
            .Build();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDto)
            .Build();

        AssertGeneratorWasNotRun(dto.GenerateCode(), bl.GenerateCode());
    }
}
