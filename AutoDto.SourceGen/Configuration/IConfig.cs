namespace AutoDto.SourceGen.Configuration;

internal interface IConfig
{
    string Key { get; }
    List<IOption> Options { get; }

    void AfterReadOptions();
}
