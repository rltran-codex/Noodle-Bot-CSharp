using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NoodleBotCSharp.Managers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NoodleBotCSharp
{
  public class NoodleBotService : IHostedService
  {
    private IServiceProvider _service;
    private DiscordSocketClient _client;
    private IConfiguration _configuration;

    private CommandService _commandService;
    private InteractionService _interactionService;

    public NoodleBotService
    (
      IServiceProvider service,
      DiscordSocketClient client,
      IConfiguration configuration,
      InteractionService interactionService,
      CommandService commandService
    )
    {
      _service = service;
      _client = client;
      _configuration = configuration;

      commandService.Log += LogManager.LogAsync;
      _commandService = commandService;

      interactionService.Log += LogManager.LogAsync;
      _interactionService = interactionService;

      _client.UserJoined += UserJoinedHandlerAsync;
      _client.Log += LogManager.LogAsync;
      _client.Ready += OnReady;
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
      _client.Ready -= OnReady;
      _interactionService.Log -= LogManager.LogAsync;
      await _client.LogoutAsync().ConfigureAwait(false);
      await _client.StopAsync().ConfigureAwait(false);
    }

    private async Task OnReady()
    {
      foreach (var guild in _client.Guilds)
      {
        LogMessage n = new(LogSeverity.Info, "Ready", $"Connected to {guild} as `{_client.CurrentUser.Username}`");
        await LogManager.LogAsync(n);
      }

      await _client.SetActivityAsync(new Game(
        name: "/repo",
        ActivityType.Watching,
        ActivityProperties.Join,
        details: "https://github.com/rltran-codex/Noodle-Bot-CSharp"
      ));
      LogMessage activityReport = new(LogSeverity.Info, "Ready", $"Activity set to `{_client.Activity.Name}`");
      await LogManager.LogAsync(activityReport);

      // delete previous commands
      await _client.Rest.DeleteAllGlobalCommandsAsync();

      // register all slash commands
      await _interactionService
            .AddModulesAsync(Assembly.GetExecutingAssembly(), _service)
            .ConfigureAwait(false);

      await _interactionService
            .RegisterCommandsGloballyAsync()
            .ConfigureAwait(false);
      _client.SlashCommandExecuted += InteractionCreated;
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
      var info = $"{interaction.Id} : {interaction.User} : {interaction.GuildId} : {interaction.GetChannelAsync()}";
      LogMessage msg = new(LogSeverity.Info, "Interaction", info);
      await LogManager.LogAsync(msg);
      var interactionContext = new SocketInteractionContext(_client, interaction);
      await _interactionService!.ExecuteCommandAsync(interactionContext, _service);
    }

    private async Task UserJoinedHandlerAsync(SocketGuildUser newUser)
    {
      try
      {
        var defaultGuild = _client.Guilds.Where(x => x.Name == "bot-lab").First();

        // assign new user the default role
        var defaultRole = defaultGuild.Roles.Where(x => x.Name == "Real-Bots").First();
        await newUser.AddRoleAsync(defaultRole);

        // create welcome message for the new user.
        var welcomeChannel = defaultGuild.SystemChannel;
        var embed = new EmbedBuilder()
            .WithTitle($"Welcome to {defaultGuild.Name}")
            .WithDescription($"Hello {newUser.Mention}, awesome of you to join us.")
            .WithAuthor(newUser.Username, newUser.GetDisplayAvatarUrl() ?? newUser.GetDefaultAvatarUrl())
            .WithColor(Color.DarkTeal)
            .WithImageUrl("https://i.kym-cdn.com/photos/images/original/001/878/329/dfa.jpg");
        await welcomeChannel.SendMessageAsync(embed: embed.Build());
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
