using AutoDto.Setup;
using AutoDto.Tests.SyntaxGeneration.Models;
using AutoDto.Tests.TestHelpers;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledBlTest : BaseCompilationErrorTests
{
    private const string _blName = "MyBl";
    private const string _blNamespace = "AutoDto.Tests.Models";

    private string GetValidDtoForBl()
    {
        var attr = typeof(DtoFromAttribute);

        return $@"
using {attr.Namespace};
using {_blNamespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ }}
";
    }

    [Fact]
    public void BrokenClassDeclarationTest()
    {
        var blModel = @$"
namespace {_blNamespace};

public cla {_blName} // incorrect 'class' word
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(blModel, GetValidDtoForBl());
    }

    [Fact]
    public void BrokenMethodDeclarationTest()
    {
        var blModel = @$"
namespace {_blNamespace};

public class {_blName}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}

    publ void MyMethod() //incorrect 'public' word
    {{
    
    }}
}}
";
        AssertGeneratorWasNotRun(blModel, GetValidDtoForBl());
    }

    [Fact]
    public void BrokenPropertyDeclarationTest()
    {
        var blModel = @$"
namespace {_blNamespace};

public class {_blName}
{{
    public int Id {{ get; set }} //missing ;
    public string Name {{ get; set; }}

    public void MyMethod()
    {{
    
    }}
}}
";
        AssertGeneratorWasNotRun(blModel, GetValidDtoForBl());
    }

    [Fact]
    public void BrokenMethodContentTest()
    {
        var blModel = @$"
namespace {_blNamespace};

public class {_blName}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}

    public void MyMethod()
    {{
        var q = 5.SomeMethod();
    }}
}}
";
        AssertGeneratorWasRunNTimes(1, 1, blModel, GetValidDtoForBl());
    }

    [Fact]
    public void NotImplementedInterfaceTest()
    {
        var interf = $@"
namespace {_blNamespace};

public interface ITest
{{
    int Id {{ get; set; }}
}}
";

        var blModel = $@"
namespace {_blNamespace};

public class {_blName} : ITest //Not implemented
{{
    public string Name {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(interf, blModel, GetValidDtoForBl());
    }

    [Fact]
    public void BrokenInheritanceDeclarationTest()
    {
        var blModelBase = $@"
namespace {_blNamespace};

public class BaseBl
{{
    public int Id {{ get; set; }}
}}
";

        var blModel = $@"
namespace {_blNamespace};

public class {_blName} : BaseBl, //Invalid ,
{{
    public string Name {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(blModelBase, blModel, GetValidDtoForBl());
    }

    [Fact]
    public void BrokenBaseBlTest()
    {
        var blModelBase = $@"
namespace {_blNamespace};

public class BaseBl
{{
    public int Id {{ get; se }} //missing t;
}}
";

        var blModel = $@"
namespace {_blNamespace};

public class {_blName} : BaseBl
{{
    public string Name {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(blModelBase, blModel, GetValidDtoForBl());
    }
}
