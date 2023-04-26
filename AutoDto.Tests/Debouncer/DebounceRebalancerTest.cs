using AutoDto.SourceGen.Debounce;

namespace AutoDto.Tests.Debouncer;

public class DebounceRebalancerTest : BaseUnitTest
{
    private class RebalanceValue
    {
        public int Count { get; set; }
        public double Value { get; set; }

        public void Reset()
        {
            Count = 0;
            Value = 0;
        }
    }
    
    private void WarmUp(DebounceRebalancer rebalancer, int n, double mainAvg, double avgLast3)
    {
        for (int i = 0; i < n; i++)
            rebalancer.AddExecutionStatistic(TimeSpan.FromMilliseconds(mainAvg));

        for (int i = 0; i < n; i++)
            rebalancer.AddExecutionStatistic(TimeSpan.FromMilliseconds(avgLast3));
    }

    [Fact]
    public void NoRebalanceTest()
    {
        var value = new RebalanceValue();
        void DoRebalance(double newPeriod)
        {
            value.Count++;
            value.Value = newPeriod;
        }

        var opts = new DebounceRebalancer.Options
        {
            LastNCount = 3,
            MinDeviation2Rebalance = 0.2,
            TimerPeriodMultiplicator = 2
        };

        var rebalancer = new DebounceRebalancer(DoRebalance, opts);

        WarmUp(rebalancer, opts.LastNCount, 100, 110);

        value.Reset();

        rebalancer.AddExecutionStatistic(TimeSpan.FromMilliseconds(120));

        Assert.Equal(0, value.Count);
    }

    [Fact]
    public void RebalanceValueTest()
    {
        var value = new RebalanceValue();
        void DoRebalance(double newPeriod)
        {
            value.Count++;
            value.Value = newPeriod;
        }

        var opts = new DebounceRebalancer.Options
        {
            LastNCount = 3,
            MinDeviation2Rebalance = 0.2,
            TimerPeriodMultiplicator = 2
        };

        var rebalancer = new DebounceRebalancer(DoRebalance, opts);

        WarmUp(rebalancer, opts.LastNCount, 100, 110);

        value.Reset();

        var newValue = 500;
        var expectedRebalance = (110*(opts.LastNCount - 1.0) + newValue)/opts.LastNCount * opts.TimerPeriodMultiplicator;

        rebalancer.AddExecutionStatistic(TimeSpan.FromMilliseconds(newValue));

        Assert.Equal(1, value.Count);

        Assert.Equal(expectedRebalance, value.Value, 0);
    }

    [Fact]
    public void RebalanceCountTest()
    {
        var value = new RebalanceValue();
        void DoRebalance(double newPeriod)
        {
            value.Count++;
            value.Value = newPeriod;
        }

        var opts = new DebounceRebalancer.Options
        {
            LastNCount = 3,
            MinDeviation2Rebalance = 0.2,
            TimerPeriodMultiplicator = 2
        };

        var rebalancer = new DebounceRebalancer(DoRebalance, opts);

        WarmUp(rebalancer, opts.LastNCount, 100, 110);

        value.Reset();

        var data = new double[]
        {
            300,
            500,
            700
        };

        foreach (var i in data)
            rebalancer.AddExecutionStatistic(TimeSpan.FromMilliseconds(i));

        Assert.Equal(3, value.Count);

        Assert.Equal(data.Average() * opts.TimerPeriodMultiplicator, value.Value, 0);
    }
}
