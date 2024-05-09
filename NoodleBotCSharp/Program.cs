using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NoodleBotCSharp.Managers;
using NoodleBotCSharp.Services;
using NoodleBotCSharp.Services.Commands;
using NoodleBotCSharp.Services.SlashCommands;

using Serilog;

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NoodleBotCSharp
{
  internal class Program
  {
    // Main entry point for starting the Discord Bot "Noodle Bot"
    private static void Main(string[] args) =>
      MainAsync(args).GetAwaiter().GetResult();

    private static async Task MainAsync(string[] args)
    {
      var log = LogManager.Instance;
      var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          config.SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables("NOODLE_");
        })
        .ConfigureServices((hostingContext, services) =>
        {
          ConfigureBotServices(services);
          services.AddLavalink();
        })
        .Build();

      try
      {
        await host.RunAsync();
      }
      catch (Exception ex)
      {
        Log.Fatal(ex.Message);
        Environment.Exit(-1);
      }
    }

    private static void ConfigureBotServices(IServiceCollection services)
    {
      services.AddSingleton<DiscordSocketConfig>(new DiscordSocketConfig()
      {
        GatewayIntents = GatewayIntents.AllUnprivileged,
        LogGatewayIntentWarnings = false,
      });

      services.AddLogging(option =>
      {
        option.AddSerilog(Log.Logger);
      });
      services.AddSingleton<DiscordSocketClient>();
      services.AddSingleton<CommandService>();
      services.Configure<InteractionServiceConfig>(c =>
      {
        c.DefaultRunMode = Discord.Interactions.RunMode.Async;
      });
      services.AddSingleton<InteractionService>();

      // add discord bot slash commands
      // services.AddSingleton<IBotSlashCommand, EchoCommand>();
      // services.AddSingleton<IBotSlashCommand, MusicCommand>();
      
      // bot service
      services.AddHostedService<NoodleBotService>();
    }
  }
}