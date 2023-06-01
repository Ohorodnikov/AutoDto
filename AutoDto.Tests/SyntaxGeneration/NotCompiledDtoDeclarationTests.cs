using AutoDto.Setup;
using AutoDto.Tests.SyntaxGeneration.Models;
using AutoDto.Tests.TestHelpers;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledDtoDeclarationTests : BaseCompilationErrorTests
{
    private const string _blName = "MyBl";

    private string GetValidBl()
    {
        var blModel = @$"
namespace {DtoCodeCreator.DtoTypNamespace};

public class {_blName}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
}}
";

        return blModel;
    }

    [Fact]
    public void BrokenOneOfPartialClassesTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code1 = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typef({_blName}))]
public partial class MyDto
{{ }}
";

        var code2 = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    publ int Id {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(code1, code2, GetValidBl());
    }

    [Fact]
    public void BrokenAttributeDefinitionTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typef({_blName}))]
public partial class MyDto
{{ }}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void BrokenKeywordsTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
puic partial class MyDto
{{ }}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void BrokenOwnPropertyTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    public strin MyOwnProperty {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void BrokenOwnProperty2()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    public string MyOwnProperty {{ get; se }}
}}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void BrokenMethodDefinitionTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    public void MyMethod(bool param1, )
    {{
    }}
}}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void PragmaErrorTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    public int Id {{ get; set; }}
#error MyTestError
}}
";

        AssertGeneratorWasNotRun(code, GetValidBl());
    }

    [Fact]
    public void BrokenMethodContentTest()
    {
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto
{{ 
    public void MyMethod(bool param1)
    {{
        var q = param1.Call();
    }}
}}
";

        AssertGeneratorWasRunNTimes(1, 1, code, GetValidBl());
    }

    [Fact]
    public void BrokenInheritanceDeclatarionTest()
    {
        var attr = typeof(DtoFromAttribute);

        var baseDtoCode = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

public class BaseDto
{{
    public string BaseDtoProp {{ get; set; }}
}}
";

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public class MyDto : BaseDto,
{{ }}
";
        AssertGeneratorWasNotRun(code, baseDtoCode, GetValidBl());
    }

    [Fact]
    public void BrokenBaseDtoTest()
    {
        var attr = typeof(DtoFromAttribute);

        var baseDtoCode = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

public partial class BaseDto
{{
    public str BaseDtoProp {{ get; set; }}
}}
";

        var code = $@"
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class MyDto : BaseDto
{{ }}
";
        AssertGeneratorWasNotRun(code, baseDtoCode, GetValidBl());
    }
}
