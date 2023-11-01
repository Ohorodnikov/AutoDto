using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledBlTest : BaseCompilationErrorTests
{
    [Fact]
    public void BrokenClassDeclarationTest()
    {
        var blName = "MyBl";
        var blModel = @$"
namespace {BlNamespace};

public cla {blName} // incorrect 'class' word
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
}}
";

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blName, BlNamespace)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(blModel, dto.GenerateCode());
    }

    [Fact]
    public void BrokenMethodDeclarationTest()
    {
        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new MethodMember("publ void MyMethod() {}"))
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void BrokenPropertyDeclarationTest()
    {
        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(new PropertyMember("public int Id2 { get; set }")) //missing ;
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void BrokenMethodContentTest()
    {
        var brokenMethod =
            new MethodBuilder("MyMethod", typeof(void))
            .SetBody("var q = 5.SomeMethod();")
            .Build();

        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .AddMember(brokenMethod)
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasRunNTimes(1, 1, bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void NotImplementedInterfaceTest()
    {
        var interf = $@"
namespace {BlNamespace};

public interface ITest
{{
    int SomeProp {{ get; set; }}
}}
";
        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddBase("ITest")
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(interf, bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void BrokenInheritanceDeclarationTest()
    {
        var blBase =
            new ClassBuilder("BlBase")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .Build();

        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddBase(blBase)
            .AddBase(" ")
            .AddMember(CommonProperties.Name)
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(blBase.GenerateCode(), bl.GenerateCode(), dto.GenerateCode());
    }

    [Fact]
    public void BrokenBaseBlTest()
    {
        var blBase =
            new ClassBuilder("BlBase")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(new PropertyMember("public int Id2 { get; se }")) //missing t;
            .Build();

        var bl = new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddBase(blBase)
            .AddMember(CommonProperties.Name)
            .Build();

        var dto = new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .Build();

        AssertGeneratorWasNotRun(blBase.GenerateCode(), bl.GenerateCode(), dto.GenerateCode());
    }
}
