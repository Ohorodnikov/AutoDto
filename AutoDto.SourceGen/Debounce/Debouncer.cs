using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace AutoDto.SourceGen.Debounce;

internal class Debouncer<TData> : IDisposable
    where TData : class
{
    private readonly Action<TData> _action;
    private readonly LogHelper _logHelper;
    private readonly IDebounceRebalancer _rebalancer;
    private Timer _timer;
    private object _lock = new object();

    public Debouncer(Action<TData> action, TimeSpan period, LogHelper logHelper, bool enableBalancer = true)
    {
        _action = action;
        _logHelper = logHelper;
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
        _logHelper.Log("Change period on " + periodInMs);

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
        _logHelper.Log($"Create timer for {periodInMs} ms");
        return new Timer((q) => Excecute(true), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(periodInMs));
    }

    private void KillCurrentTimer()
    {
        _logHelper.Log("Kill timer and start sync mode");
        _timer?.Dispose();
        _timer = null;
    }

    private void Excecute(bool inTimer)
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
                if (inTimer)
                {
                    _logHelper.Log("Run action in timer: " + runEvent.Id.ToString());
                }
                else
                {
                    _logHelper.Log("Run action NOW: " + runEvent.Id.ToString());
                }
                sw.Start();
                _action(runEvent.Data);
                sw.Stop();
            }
            catch (Exception e)
            {
                _logHelper.Log(e.Message);
                //LogHelper.Log(e.StackTrace);
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

        _logHelper.Log("Publish event: " + runEvent.Id.ToString());

        return runEvent;
    }

    public void RunAction(TData data)
    {
        if (!IsTimerAlive())
        {
            _action(data);
            return;
        }

        var runEvent = PublishEvent(data);

        while (!runEvent.IsFinished)
        {
            Thread.Sleep(50);
        }

        _logHelper.Log($"Event {runEvent.Id} finished");
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
