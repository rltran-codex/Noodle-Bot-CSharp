using Discord;

using Serilog;
using Serilog.Events;

using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;

namespace NoodleBotCSharp.Managers
{
  public class LogManager
  {
    // Private static instance variable
    private static LogManager _instance;
    private readonly string _logFile;
    private LogManager()
    {
      var logfn = $"noodlebot_{DateTime.Now.ToString("yyyyMMdd")}.txt";
      _logFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "NoodleBot",
        "Logs",
        logfn
        );
      Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(_logFile)
        .CreateLogger();

      Log.Write(LogEventLevel.Information, "Logger successfully initialized");
      Log.Write(LogEventLevel.Information, $"Log file can be found at: {_logFile}");
    }

    public static LogManager Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new LogManager();
        }
        return _instance;
      }
    }

    public static async Task LogAsync(LogMessage msg)
    {
      await Task.Run(() =>
      {
        var severity = msg.Severity switch
        {
          LogSeverity.Critical => LogEventLevel.Fatal,
          LogSeverity.Error => LogEventLevel.Error,
          LogSeverity.Warning => LogEventLevel.Warning,
          LogSeverity.Info => LogEventLevel.Information,
          LogSeverity.Verbose => LogEventLevel.Verbose,
          LogSeverity.Debug => LogEventLevel.Debug,
          _ => LogEventLevel.Information
        };

        Log.Write(severity, msg.Exception, "[{Source}] {Message}", msg.Source, msg.Message);
      });
    }
  }
}
