namespace AutoDto.Tests.SyntaxGeneration;

public class BaseCompilationErrorTests : BaseUnitTest
{
    protected void AssertGeneratorWasRunNTimes(int n, params string[] classCodes)
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

    protected void AssertGeneratorWasNotRun(params string[] dtoCodes)
    {
        AssertGeneratorWasRunNTimes(0, dtoCodes);
    }
}
