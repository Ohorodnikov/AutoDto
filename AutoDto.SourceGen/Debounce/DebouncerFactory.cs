using AutoDto.SourceGen.Configuration;
using System.Collections.Concurrent;

namespace AutoDto.SourceGen.Debounce;

internal static class DebouncerFactory<TData>
        where TData : class
{
    static ConcurrentDictionary<Action<TData>, IDebouncer<TData>> _cache = new ConcurrentDictionary<Action<TData>, IDebouncer<TData>>();

    public static IDebouncer<TData> GetForAction(Action<TData> action, DebouncerConfig config = null)
    {
        var cfg = config ?? GlobalConfig.Instance.DebouncerConfig;
        return _cache.GetOrAdd(action, (key) => Create(key, cfg));
    }

    public static IDebouncer<TData> Create(Action<TData> action, DebouncerConfig config)
    {
        if (config == null)
            throw new ArgumentNullException("config");

        return new Debouncer<TData>(action, TimeSpan.FromMilliseconds(config.IntervalMs), config.UseDebouncer, config.AllowAutoRebalance);
    }
}
