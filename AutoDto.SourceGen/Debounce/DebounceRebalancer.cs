namespace AutoDto.SourceGen.Debounce;

internal interface IDebounceRebalancer
{
    void AddExecutionStatistic(TimeSpan elapsed);
}

internal class DisabledDebounceRebalancer : IDebounceRebalancer
{
    public void AddExecutionStatistic(TimeSpan elapsed)
    {
    }
}

internal class DebounceRebalancer : IDebounceRebalancer
{
    internal class Options
    {
        public int LastNCount { get; set; }
        public double MinDeviation2Rebalance { get; set; }
        public double TimerPeriodMultiplicator { get; set; }
    }
   
    private AvgCalculator _avgCalculator;
    private Options _options;
    private readonly Action<double> _newPeriodSetter;

    public DebounceRebalancer(Action<double> newPeriodSetter, Options options)
    {
        _avgCalculator = new AvgCalculator(options.LastNCount, "time");
        _options = options;
        _newPeriodSetter = newPeriodSetter;
    }

    public void AddExecutionStatistic(TimeSpan elapsed)
    {
        var elapsedMs = elapsed.TotalMilliseconds;

        _avgCalculator.Push(elapsedMs);

        var dev = _avgCalculator.GetDeviationForLastN();
        if (dev > _options.MinDeviation2Rebalance)
            RebalanceDebouncerTimer();
    }

    private void RebalanceDebouncerTimer()
    {
        var lastNAvg = _avgCalculator.GetLastNAvgValue();

        LogHelper.Logger.Debug("Avg time for last exec: {time}", lastNAvg);

        var newPeriod = CalculateNewPeriod(lastNAvg);

        _newPeriodSetter(newPeriod);
    }

    private double CalculateNewPeriod(double execAvgTime)
    {
        return execAvgTime * _options.TimerPeriodMultiplicator;
    }
}
