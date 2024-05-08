using Discord;

using Serilog;
using Serilog.Events;

using System;
using System.Threading.Tasks;
using System.IO;

namespace NoodleBotCSharp.Managers
{
  public class LogManager
  {
    // Private static instance variable
    private static LogManager _instance;

    private LogManager()
    {
      var logfn = $"noodlebot_{DateTime.Now.ToString("yyyyMMdd")}.txt";
      var logFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "NoodleBot",
        "Logs",
        logfn
        );
      Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(logFile)
        .CreateLogger();

      Log.Write(LogEventLevel.Information, "Logger successfully initialized");
      Log.Write(LogEventLevel.Information, $"Log file can be found at: {logFile}");
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

    public static Task LogAsync(LogMessage msg)
    {
      return Task.Run(() =>
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
