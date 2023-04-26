namespace AutoDto.SourceGen.Configuration;

internal class DebouncerConfig : IConfig
{
    public DebouncerConfig()
    {
        Options = new List<IOption>
        {
            OptionFactory.New("enabled", true, SetUseDebouncer),
            OptionFactory.New("interval", 500, SetInterval),
            OptionFactory.New("auto_rebalance_enabled", true, SetAllowAutoRebalance),
        };
    }
    public string Key => "debounce";
    public List<IOption> Options { get; }

    public bool UseDebouncer { get; private set; }
    public int IntervalMs { get; private set; }
    public bool AllowAutoRebalance { get; private set; }

    private void SetUseDebouncer(string value)
    {
        if (bool.TryParse(value, out bool isEnabled))
            UseDebouncer = isEnabled;
    }

    private void SetAllowAutoRebalance(string value)
    {
        if (bool.TryParse(value, out bool isEnabled))
            AllowAutoRebalance = isEnabled;
    }

    private void SetInterval(string value)
    {
        if (int.TryParse(value, out var v) && v > 0)
            IntervalMs = v;            
    }

    public void AfterReadOptions() { }
}
