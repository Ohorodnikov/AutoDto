using AutoDto.SourceGen.Configuration.Configs;
using Microsoft.CodeAnalysis.Diagnostics;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.Configuration;

internal class GlobalConfig
{
    private const string BASE_KEY = "auto_dto";
    private static readonly Lazy<GlobalConfig> _instance = new Lazy<GlobalConfig>(() => new GlobalConfig(), LazyThreadSafetyMode.PublicationOnly);

    public static GlobalConfig Instance => _instance.Value;

    public static AnalyzerConfigOptions GlobalOptions { get; set; }

    private GlobalConfig()
    {
        string defLogFolder = null;

        if (GlobalOptions != null && GlobalOptions.TryGetValue("build_property.projectdir", out var val))
            defLogFolder = val;

        LoggerConfig = new FileLoggerConfig(defLogFolder ?? Path.Combine(Environment.CurrentDirectory, "Logs"));
        DebouncerConfig = new DebouncerConfig();
    }

    public FileLoggerConfig LoggerConfig { get; }
    public DebouncerConfig DebouncerConfig { get; }

    public bool IsInited { get; private set; }
    private object _lock = new object();
    public void Init(AnalyzerConfigOptions configOptions)
    {
        if (IsInited) 
            return;

        lock (_lock)
        {
            if (IsInited)
                return;

            ReadConfig(LoggerConfig, configOptions);
            ReadConfig(DebouncerConfig, configOptions);

            IsInited = true;
        }
    }

    private void ReadConfig(IConfig config, AnalyzerConfigOptions configOptions)
    {
        foreach (var opt in config.Options)
            ReadOption(config, configOptions, opt);        

        config.AfterReadOptions();
    }

    private void ReadOption(IConfig config, AnalyzerConfigOptions configOptions, IOption opt)
    {
        try
        {
            var key = BuildKey(config, opt.Key);
            if (configOptions.TryGetValue(key, out var val))
            {
                opt.Setter(val);
                LogHelper.Log(LogEventLevel.Debug, "read option '{opt}': {val}", key, val);
            }
            else
            {
                opt.SetDefault();
                LogHelper.Log(LogEventLevel.Debug, "not found option '{opt}'", key);
            }
        }
        catch (Exception e)
        {
            LogHelper.Log(LogEventLevel.Error, "Error during reading or setting option: {configKey}.{optionKey}", config.Key, opt.Key);
            LogHelper.Log(LogEventLevel.Error, "Error: {msg}", e.Message);
        }
    }

    private static string BuildKey(IConfig config, string optionKey) 
        => string.Join(".", BASE_KEY, config.Key, optionKey);
}
