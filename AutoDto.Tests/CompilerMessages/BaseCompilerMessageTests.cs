using Microsoft.CodeAnalysis;

namespace AutoDto.Tests.CompilerMessages;

public class BaseCompilerMessageTests : BaseUnitTest
{
    protected void AssertMessage(DiagnosticSeverity expectedSeverity, string expectedId, Diagnostic message)
    {
        Assert.Equal(expectedSeverity, message.Severity);
        Assert.Equal(expectedId, message.Id);
    }
}
