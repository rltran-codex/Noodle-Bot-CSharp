using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using NoodleBotCSharp.Managers;
using NoodleBotCSharp.Services.Commands;
using NoodleBotCSharp.Services.SlashCommands;

using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NoodleBotCSharp
{
  public class NoodleBotService : IHostedService
  {
    private IServiceProvider _service;
    private DiscordSocketClient _client;
    private IConfiguration _configuration;
    
    private IEnumerable<IBotSlashCommand> _slashCommands;
    private Dictionary<string, IBotSlashCommand> _commandMap = new Dictionary<string, IBotSlashCommand>();

    public NoodleBotService(IServiceProvider service, DiscordSocketClient client, IConfiguration configuration, IEnumerable<IBotSlashCommand> BotSlashCommands)
    {
      _service = service;
      _client = client;
      _configuration = configuration;
      _slashCommands = BotSlashCommands;

      _client.Log += LogManager.LogAsync;
      _client.Ready += OnReady;
      _client.SlashCommandExecuted += OnSlashCommandExecutedAsync;
      service.GetRequiredService<CommandService>().Log += LogManager.LogAsync;
      service.GetRequiredService<InteractionService>().Log += LogManager.LogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var token = _configuration.GetValue<string>("CLIENT_TOKEN");
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      await _client.LogoutAsync();
    }

    private async Task OnReady()
    {
      foreach (var guild in _client.Guilds)
      {
        LogMessage n = new(LogSeverity.Info, "Ready", $"Connected to {guild} as `{_client.CurrentUser.Username}`");
        await LogManager.LogAsync(n);
      }

      await _client.SetGameAsync("/help", type: ActivityType.Playing);
      LogMessage activityReport = new(LogSeverity.Info, "Ready", $"Activity set to `{_client.Activity.Name}`");
      await LogManager.LogAsync(activityReport);

      // delete previous commands
      // await _client.Rest.DeleteAllGlobalCommandsAsync();

      // register all slash commands
      foreach (var slashCommand in _slashCommands)
      {
        var slashBuilder = slashCommand.GetSlashCommandBuilder();
        _commandMap.Add(slashBuilder.Name, slashCommand);
        try
        {
          await _client.CreateGlobalApplicationCommandAsync(slashBuilder.Build());
          Log.Information($"Registered slash command /{slashBuilder.Name}");
        }
        catch (Exception ex)
        {
          Log.Error(ex.InnerException, $"Unable to register slash command {slashBuilder.Name}");
        }
      }
    }

    private async Task OnSlashCommandExecutedAsync(SocketSlashCommand command)
    {
      if (!_commandMap.ContainsKey(command.CommandName))
      {
        return;
      }
      var botSlashCommand = _commandMap[command.CommandName];
      await botSlashCommand.SlashCommandHandler(command);
    }
  }
}
