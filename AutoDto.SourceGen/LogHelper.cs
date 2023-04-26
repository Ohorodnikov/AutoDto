using AutoDto.SourceGen.Configuration.Configs;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoDto.SourceGen;

internal class LogHelper
{
    private class LogEvent
    {
        public LogEvent(LogEventLevel level, string template, params object[] args)
        {
            Level = level;
            Template = template;
            Args = args;
        }

        public LogEventLevel Level { get; }
        public string Template { get; }
        public object[] Args { get; }
    }

    /// <summary>
    /// Must be used when not sure if logger was already inited
    /// </summary>
    /// <param name="level"></param>
    /// <param name="template"></param>
    /// <param name="args"></param>
    public static void Log(LogEventLevel level, string template, params object[] args)
    {
        if (Logger != null)
        {
            Logger.Write(level, template, args);
            return;
        }
        _beforeLoggerInitMessages.Enqueue(new LogEvent(level, template, args));
    }
    private static ConcurrentQueue<LogEvent> _beforeLoggerInitMessages = new ConcurrentQueue<LogEvent>();

    /// <summary>
    /// Can be null. It is posseble before logger inited
    /// </summary>
    public static ILogger Logger { get; private set; }

    public static void InitFileLogger(FileLoggerConfig config)
    {
        var loggerConf = new LoggerConfiguration();

        if (config != null && config.IsEnabled)
        {
            Directory.CreateDirectory(config.FolderPath);

            loggerConf = loggerConf
                .MinimumLevel.Verbose()
                .WriteTo
                .File(Path.Combine(config.FolderPath, "AutoDto.log"), config.LogLevel, shared: true);
        }

        Logger = loggerConf.CreateLogger();

        while (_beforeLoggerInitMessages.TryDequeue(out var data))
            Logger.Write(data.Level, data.Template, data.Args);

        _beforeLoggerInitMessages = null; //free memory for empty ConcurrentQueue
    }
    
    public static void InitDebugLogger()
    {
        Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo
            .Debug()
            .CreateLogger();
    }
}
