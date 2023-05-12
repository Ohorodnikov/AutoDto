namespace AutoDto.SourceGen.Debounce;

internal interface IDebounceEvent
{
    Guid Id { get; }
    bool IsFinished { get; }

    object GetData();

    void Finish();
    void Wait();
}

internal class DebounceEvent<TData> : IDebounceEvent 
    where TData : class
{
    public DebounceEvent(TData data)
    {
        Data = data;
        Id = Guid.NewGuid();
        IsFinished = false;
    }
    public TData Data { get; }
    public Guid Id { get; }
    public bool IsFinished { get; private set; }

    public void Wait()
    {
        while (!IsFinished) { }
    }

    public void Finish()
    {
        IsFinished = true;
    }

    public object GetData()
    {
        return Data;
    }
}
