using Serilog.Events;
using System.IO;

namespace AutoDto.SourceGen.Configuration.Configs;

internal class FileLoggerConfig : IConfig
{
    public FileLoggerConfig(string defaultFolder)
    {
#if DEBUG
        var defLogLevel = LogEventLevel.Debug;
#else
        var defLogLevel = LogEventLevel.Warning;
#endif
        Options = new List<IOption>
        {
            OptionFactory.New("enabled", false, SetIsEnabled),
            OptionFactory.New("log_level", defLogLevel, SetLogLevel),
            OptionFactory.New("folder_path", defaultFolder, SetFolderPath),
        };
    }
    public string Key => "logger";
    public List<IOption> Options { get; }

    public bool IsEnabled { get; private set; }
    public LogEventLevel LogLevel { get; private set; }
    public string FolderPath { get; private set; }

    private void SetIsEnabled(string value)
    {
        if (bool.TryParse(value, out bool isEnabled))
            IsEnabled = isEnabled;
    }

    private void SetLogLevel(string value)
    {
        if (Enum.TryParse<LogEventLevel>(value, true, out var level))
            LogLevel = level;
    }

    private void SetFolderPath(string value)
    {
        FolderPath = value;
    }

    public void AfterReadOptions()
    {
        if (LogHelper.Logger == null)
            LogHelper.InitFileLogger(this);
    }
}
