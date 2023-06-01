using AutoDto.Setup;
using AutoDto.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.SyntaxGeneration;

public class CompilationErrorsNotRelatedToDtoTest : BaseCompilationErrorTests
{
    private const string _blName = "MyBl";
    private const string _blNamespace = "AutoDto.Tests.Models";

    private string GetValidDtoForBl(string dtoName = "MyDto")
    {
        var attr = typeof(DtoFromAttribute);

        return $@"
using {attr.Namespace};
using {_blNamespace};

namespace {DtoCodeCreator.DtoTypNamespace};

[DtoFrom(typeof({_blName}))]
public partial class {dtoName}
{{ }}
";
    }

    private string GetValidBl()
    {
        var blModel = @$"
namespace {_blNamespace};

public class {_blName}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }}
}}
";

        return blModel;
    }

    [Fact]
    public void CompilationErrorInFileWithOneDto()
    {
        var dtoFileContent = $@"
{GetValidDtoForBl()}

public class SomeOtherClass
{{
    public voi SomeMethod() {{ }} //wrong 'void'
}}
";

        AssertGeneratorWasRunNTimes(1, 1, GetValidBl(), dtoFileContent);
    }

    [Fact]
    public void CompilationErrorInFileWithManyDto()
    {
        var dtoFileContent = $@"
{GetValidDtoForBl("Dto1")}

[DtoFrom(typeof({_blName}))]
public partial class Dto2
{{ }}

public class SomeOtherClass
{{
    public voi SomeMethod() {{ }} //wrong 'void'
}}
";

        AssertGeneratorWasRunNTimes(1, 2, GetValidBl(), dtoFileContent);
    }

    [Fact]
    public void CompilationErrorInFileWithBl() 
    {
        var blFileContent = $@"
{GetValidBl()}

public class SomeOtherClass
{{
    public voi SomeMethod() {{ }} //wrong 'void'
}}
";

        AssertGeneratorWasRunNTimes(1, 1,blFileContent, GetValidDtoForBl());
    }

    [Fact]
    public void CompilationErrorInFileNotRelatedToDto() 
    {
        var extraFile = $@"
public class SomeOtherClass
{{
    public voi SomeMethod() {{ }} //wrong 'void'
}}
";
        AssertGeneratorWasRunNTimes(1, 1, GetValidBl(), GetValidDtoForBl(), extraFile);
    }
}
