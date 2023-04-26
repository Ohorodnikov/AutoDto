using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace AutoDto.SourceGen.Debounce;

internal interface IDebouncer<TData> where TData : class
{
    void RunAction(TData data);
}

internal class DebouncerFake<TData> : IDebouncer<TData> where TData : class
{
    private readonly Action<TData> _action;

    public DebouncerFake(Action<TData> action)
    {
        _action = action;
    }
    public void RunAction(TData data)
    {
        _action(data);
    }
}

internal class Debouncer<TData> : IDisposable, IDebouncer<TData> where TData : class
{
    private readonly Action<TData> _action;
    private readonly IDebounceRebalancer _rebalancer;
    private Timer _timer;
    private object _lock = new object();

    public Debouncer(Action<TData> action, TimeSpan period, bool enableBalancer = true)
    {
        _action = action;
        _runEvents = new ConcurrentStack<RunEvent>();

        var rebalancerOpts = new DebounceRebalancer.Options
        {
            LastNCount = 10,
            MinDeviation2Rebalance = 0.2,
            TimerPeriodMultiplicator = 2
        };

        if (enableBalancer)
            _rebalancer = new DebounceRebalancer(InitTimer, rebalancerOpts);
        else
            _rebalancer = new DisabledDebounceRebalancer();

        InitTimer(period.TotalMilliseconds);
    }

    private bool IsTimerAlive() => _timer is not null;

    private void InitTimer(double periodInMs)
    {
        LogHelper.Logger.Warning("Change period on {period} ms", periodInMs);

        if (periodInMs < 250)
        {
            KillCurrentTimer();
            return;
        }

        if (IsTimerAlive())
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(periodInMs));
        else
            _timer = CreateNewTimer(periodInMs);
    }

    private Timer CreateNewTimer(double periodInMs)
    {
        LogHelper.Logger.Information("Create timer for {periodInMs} ms", periodInMs);
        return new Timer((q) => Execute(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(periodInMs));
    }

    private void KillCurrentTimer()
    {
        LogHelper.Logger.Information("Kill timer and start sync mode");
        _timer?.Dispose();
        _timer = null;
    }

    private void Execute()
    {
        if (_runEvents.Count == 0)
            return;

        lock (_lock)
        {
            if (_runEvents.Count == 0)
                return;

            if (!_runEvents.TryPeek(out var runEvent))
                return;

            var sw = new Stopwatch();
            try
            {
                LogHelper.Logger.Debug("Run event: {id}", runEvent.Id);

                sw.Start();
                _action(runEvent.Data);
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error(e, "error happened");
            }
            finally
            {
                sw.Stop();
            }

            while (_runEvents.TryPop(out var v))
                v.Finish();

            _rebalancer.AddExecutionStatistic(sw.Elapsed);
        }
    }

    private class RunEvent
    {
        public RunEvent(TData data)
        {
            Data = data;
            Id = Guid.NewGuid();
            IsFinished = false;
        }
        public TData Data { get; }
        public Guid Id { get; }
        public bool IsFinished { get; private set; }

        public void Finish()
        {
            IsFinished = true;
        }
    }

    private ConcurrentStack<RunEvent> _runEvents;
    private RunEvent PublishEvent(TData data)
    {
        var runEvent = new RunEvent(data);

        _runEvents.Push(runEvent);

        LogHelper.Logger.Information("Publish event: {id}", runEvent.Id);

        return runEvent;
    }

    public void RunAction(TData data)
    {
        if (!IsTimerAlive())
        {
            LogHelper.Logger.Information($"Execute without debounce");
            _action(data);
            return;
        }

        var runEvent = PublishEvent(data);

        while (!runEvent.IsFinished)
            Thread.Sleep(50);

        LogHelper.Logger.Information($"Event {runEvent.Id} finished");
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
