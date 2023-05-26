using AutoDto.Setup;
using AutoDto.Tests.SyntaxGeneration.Models;
using AutoDto.Tests.TestHelpers;

namespace AutoDto.Tests.SyntaxGeneration;

public class NotCompiledBlTest : BaseUnitTest
{
    private void AssertGeneratorWasRunNTimes(int n, params string[] classCodes)
    {
        int i = 0;
        void CalculateExecCount()
        {
            Interlocked.Increment(ref i);
        }

        var gen = GetGeneratorConfigured(false, CalculateExecCount);

        var (compilations, msgs) = gen.RunWithMsgs(classCodes);

        Assert.Equal(n, i);

        Assert.Equal(classCodes.Length, compilations.SyntaxTrees.Count());
    }

    private void AssertGeneratorWasNotRun(params string[] classCodes)
    {
        AssertGeneratorWasRunNTimes(0, classCodes);
    }

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
public class MyDto
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
        AssertGeneratorWasRunNTimes(1, blModel, GetValidDtoForBl());
        //AssertGeneratorWasNotRun(blModel, GetValidDtoForBl()); //TODO: must run once
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
