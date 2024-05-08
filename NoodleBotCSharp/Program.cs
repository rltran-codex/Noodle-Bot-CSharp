
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NoodleBotCSharp.Managers;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NoodleBotCSharp
{
  public class Program
  {
    // Main entry point for starting the Discord Bot "Noodle Bot"
    public static async Task Main(string[] args)
    {
      var log = LogManager.Instance;

      // build host service
      var builder = new HostBuilder()
        .UseSerilog() // attaching Serilog
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          // configurations for app
          config.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
          // services for app
          services.AddSingleton<IHostedService, NoodleBotService>();
        });

      try
      {
        await builder.RunConsoleAsync();
      }
      catch (OperationCanceledException ocex)
      {
        Console.WriteLine(ocex.Message);
      }
    }
  }
}