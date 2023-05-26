using AutoDto.Setup;
using AutoDto.Tests.SyntaxGeneration.Models;
using AutoDto.Tests.TestHelpers;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledDtoDeclarationTests : BaseCompilationErrorTests
{
    [Fact]
    public void BrokenAttributeDefinitionTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typef({nameof(SimpleType)}))]
public class MyDto
{{ }}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void BrokenKeywordsTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
puic class MyDto
{{ }}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void BrokenOwnPropertyTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto
{{ 
    public strin MyOwnProperty {{ get; set; }}
}}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void BrokenOwnProperty2()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto
{{ 
    public string MyOwnProperty {{ get; se }}
}}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void BrokenMethodDefinitionTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto
{{ 
    public void MyMethod(bool param1, )
    {{
    }}
}}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void PragmaErrorTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto
{{ 
    public int Id {{ get; set; }}
#error MyTestError
}}
";

        AssertGeneratorWasNotRun(code);
    }

    [Fact]
    public void BrokenMethodContentTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto
{{ 
    public void MyMethod(bool param1)
    {{
        var q = param1.Call();
    }}
}}
";

        AssertGeneratorWasRunNTimes(1, code);
    }

    [Fact]
    public void BrokenInheritanceDeclatarionTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var baseDtoCode = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

public class BaseDto
{{
    public string BaseDtoProp {{ get; set; }}
}}
";

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto : BaseDto,
{{ }}
";
        AssertGeneratorWasNotRun(code, baseDtoCode);
    }

    [Fact]
    public void BrokenBaseDtoTest()
    {
        var type = typeof(SimpleType);
        var attr = typeof(DtoFromAttribute);

        var baseDtoCode = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

public class BaseDto
{{
    public str BaseDtoProp {{ get; set; }}
}}
";

        var code = $@"
using {type.Namespace};
using {attr.Namespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({nameof(SimpleType)}))]
public class MyDto : BaseDto
{{ }}
";
        AssertGeneratorWasNotRun(code, baseDtoCode);
    }
}
