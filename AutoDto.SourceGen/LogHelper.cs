using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen;

internal class LogHelper
{
    public static ILogger Logger { get; private set; }

    public static void InitFileLogger(string path)
    {
#if DEBUG
        var logLevel = Serilog.Events.LogEventLevel.Debug;
#else
        var logLevel = Serilog.Events.LogEventLevel.Warning;
#endif

        Logger = new LoggerConfiguration()
            .WriteTo
            .File(Path.Combine(path, "AutoDto.log"), logLevel, shared: true)
            .CreateLogger();
    }
    
    public static void InitDebugLogger()
    {
        Logger = new LoggerConfiguration()
            .WriteTo
            .Debug()
            .CreateLogger();
    }
}
