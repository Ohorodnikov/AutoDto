using AutoDto.Tests.TestHelpers;

namespace AutoDto.Tests.SyntaxGeneration;

public class BaseCompilationErrorTests : BaseUnitTest
{
    protected void AssertGeneratorWasRunNTimes(int n, int expectedResultCount, params string[] classCodes)
    {
        if (expectedResultCount < 0)
            throw new ArgumentException("expectedResultCount must be not less than 0");

        int i = 0;
        void CalculateExecCount()
        {
            Interlocked.Increment(ref i);
        }

        var gen = new GeneratorRunner
        {
            CheckInputCompilation = false,
            OnApplyGenerator = CalculateExecCount
        };

        var (compilations, msgs) = gen.Run(classCodes);

        Assert.Equal(n, i);

        Assert.Equal(classCodes.Length + expectedResultCount, compilations.SyntaxTrees.Count());
    }

    protected void AssertGeneratorWasNotRun(params string[] dtoCodes)
    {
        AssertGeneratorWasRunNTimes(0, 0, dtoCodes);
    }
}
