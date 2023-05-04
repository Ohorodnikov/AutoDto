namespace AutoDto.SourceGen.Debounce;

public class AvgCalculator
{
    private const double MAX_DOUBLE = double.MaxValue * 0.9;
    private double _count = 0;
    private double _avgValue = 0;
    private readonly int _lastN;
    private readonly string _decription;
    private readonly Queue<double> _lastNQueue;
    private readonly object _lock = new object();

    public AvgCalculator(int lastN, string decription)
    {
        _lastN = lastN;
        _decription = decription;
        _lastNQueue = new Queue<double>();
    }

    public void Push(double value)
    {
        lock (_lock)
        {
            LogHelper.Logger.Debug("Get value {value} for " + _decription, value);
            while (_lastNQueue.Count >= _lastN) //hope always _lastNQueue.Count == _lastN
                RecalculateAvg(_lastNQueue.Dequeue());

            _lastNQueue.Enqueue(value);
        }
    }

    private void RecalculateAvg(double newValue)
    {
        _avgValue = (_count * _avgValue + newValue) / (_count + 1);

        if (_count % 10 == 0)
            LogHelper.Logger.Debug($"Average {_decription}:" + " {time}", _avgValue);

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
        if (_lastNQueue.Count == 0)
            return 0.0;

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
