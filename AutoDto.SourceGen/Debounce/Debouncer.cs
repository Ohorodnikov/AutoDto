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
    IDebounceEvent RunAction(TData data);
}

internal class Debouncer<TData> : IDebouncer<TData> where TData : class
{
    private const int MINIMAL_TIME_TO_RUN_DEBOUNCED = 100;

    private readonly AvgCalculator _eventsCountAvg;

    private readonly Action<TData> _action;
    private readonly bool _alwaysImmediately;
    private readonly IDebounceRebalancer _rebalancer;
    private int _time2CollectEvents = 0;

    public Debouncer(Action<TData> action, TimeSpan period, bool alwaysImmediately, bool enableBalancer = true)
    {
        _action = action;
        _alwaysImmediately = alwaysImmediately;
        _runEvents = new ConcurrentStack<IDebounceEvent>();
        _eventsCountAvg = new AvgCalculator(50, "events");
        SetPeriod(period.TotalMilliseconds);

        var rebalancerOpts = new DebounceRebalancer.Options
        {
            LastNCount = 10,
            MinDeviation2Rebalance = 0.2,
            TimerPeriodMultiplicator = 2
        };

        _rebalancer = enableBalancer 
                    ? new DebounceRebalancer(SetPeriod, rebalancerOpts) 
                    : new DisabledDebounceRebalancer();
    }

    private void SetPeriod(double period)
    {
        LogHelper.Logger.Warning("Set debouncer period to {period}", period);
        _time2CollectEvents = (int)period;
    }

    private ConcurrentStack<IDebounceEvent> _runEvents;
    private void PublishEvent(IDebounceEvent debounceEvent)
    {
        _runEvents.Push(debounceEvent);

        LogHelper.Logger.Information("Publish event: {id}", debounceEvent.Id);
    }

    public IDebounceEvent RunAction(TData data)
    {
        var ev = new DebounceEvent<TData>(data);
        if (IsRunImmediately())
        {
            LogHelper.Logger.Debug("Run immediately");
            ExecuteSafe(ev);
            ev.Finish();
        }
        else
        {
            PublishEvent(ev);
            Execute();
        }
       
        return ev;
    }

    private bool IsRunImmediately()
    {
        if (_alwaysImmediately)
            return true;

        return _time2CollectEvents < MINIMAL_TIME_TO_RUN_DEBOUNCED && _eventsCountAvg.GetLastNAvgValue() < 1.5;
    }

    private bool _isCollectingEvents = false;
    private readonly object _createTaskLock = new object();
    private void Execute()
    {
        if (_isCollectingEvents)
            return;

        lock (_createTaskLock)
        {
            if (_isCollectingEvents)
                return;

            _isCollectingEvents = true;

            Task.Factory.StartNew(() =>
            {
                var events = CollectEvents();
                _isCollectingEvents = false;

                if (events.Count == 0)
                    return;

                _eventsCountAvg.Push(events.Count);
                ExecuteSafe(events.Peek());

                FinishCollectedEvents(events);
            });
        }
    }

    private void DebounceWait()
    {
        Thread.Sleep(_time2CollectEvents);
    }

    private Queue<IDebounceEvent> CollectEvents()
    {
        DebounceWait();

        var currQueue = new Queue<IDebounceEvent>();

        while (_runEvents.TryPop(out var v))
            currQueue.Enqueue(v);

        LogHelper.Logger.Debug("Collected {count} events", currQueue.Count);

        return currQueue;
    }

    private void ExecuteSafe(IDebounceEvent runEvent)
    {
        try
        {
            LogHelper.Logger.Debug("Run event: {id}", runEvent.Id);

            var sw = new Stopwatch();
            sw.Start();
            _action((TData)runEvent.GetData());
            sw.Stop();

            LogHelper.Logger.Debug("Running event {id} takes {ms} ms", runEvent.Id, sw.ElapsedMilliseconds);

            _rebalancer.AddExecutionStatistic(sw.Elapsed);
        }
        catch (Exception e)
        {
            LogHelper.Logger.Error(e, "error happened");
        }
    }

    private void FinishCollectedEvents(Queue<IDebounceEvent> events)
    {
        while (events.Count > 0)
            events.Dequeue().Finish();
    }
}
