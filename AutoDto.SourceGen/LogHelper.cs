//using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen;

internal class LogHelper
{
    //static ILogger logger;
    //static LogHelper()
    //{
    //    logger = new LoggerConfiguration()
    //        .WriteTo
    //        .File(LogFilePath + "log.txt", outputTemplate: "{Message}{NewLine}")
    //        .CreateLogger();
    //}
    public LogHelper(string id, string guid)
    {
        this.id = id;
        this.guid = guid;
    }
    private const string LogFilePath = "C:/Data/CustomTools/AutoDto/AutoDto.SourceGen/Logs/";
    private readonly string id;
    private readonly string guid;
    private List<string> messages = new List<string>();

    private object _lock = new object();

    public virtual void Log(string message)
    {
        //var msg = 
        //messages.Add($"{DateTime.Now.ToString("hh:mm:ss.fff")} : {message}");

        var msg = $"{DateTime.Now.ToString("hh:mm:ss.fff")} : {id}_{guid} : {message}";

        //logger.Information(msg);

        File.AppendAllLines(LogFilePath + $"log{id}_{guid}.txt", new[] { msg });

    }
}
