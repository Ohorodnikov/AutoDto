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

    private class AvgCalculator
    {
        private const double MAX_DOUBLE = double.MaxValue * 0.9;
        private double _count = 0;
        private double _avgValue = 0;
        private readonly int _lastN;
        private readonly Queue<double> _lastNQueue;
        private readonly object _lock = new object();

        public AvgCalculator(int lastN)
        {
            _lastN = lastN;
            _lastNQueue = new Queue<double>();
        }

        public void Push(double value)
        {
            lock (_lock)
            {
                while (_lastNQueue.Count >= _lastN) //hope always _lastNQueue.Count == _lastN
                    RecalculateAvg(_lastNQueue.Dequeue());                

                _lastNQueue.Enqueue(value);
            }
        }

        private void RecalculateAvg(double newValue)
        {
            _avgValue = (_count * _avgValue + newValue) / (_count + 1);

            if (_count % 10 == 0)
                LogHelper.Logger.Debug("Average time: {time}",_avgValue);

            IncementCount();
        }

        private void IncementCount()
        {
            _count++;

            if (_count > MAX_DOUBLE)
                _count = 100_000_000; //still big enought to not change average calculation
        }

        public double GetLastNAvgValue()
        {
            return _lastNQueue.Average();
        }

        public double GetAvgValue()
        {
            return _avgValue;
        }

        public double GetDeviationFromAvg(double currentValue)
        {
            if (_avgValue == 0)
                return 0;

            return Math.Abs(_avgValue - currentValue) / _avgValue;
        }

        public double GetDeviationForLastN() => GetDeviationFromAvg(GetLastNAvgValue());
    }

    private AvgCalculator _avgCalculator;
    private Options _options;
    private readonly Action<double> _newPeriodSetter;

    public DebounceRebalancer(Action<double> newPeriodSetter, Options options)
    {
        _avgCalculator = new AvgCalculator(options.LastNCount);
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

        var newPeriod = CalculateNewPeriod(lastNAvg);

        _newPeriodSetter(newPeriod);
    }

    private double CalculateNewPeriod(double execAvgTime)
    {
        return execAvgTime * _options.TimerPeriodMultiplicator;
    }
}
