using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NoodleBotCSharp.Managers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NoodleBotCSharp
{
  public class NoodleBotService : IHostedService
  {
    private DiscordSocketClient _client;
    private readonly ILogger<NoodleBotService> _logger;

    public NoodleBotService(ILogger<NoodleBotService> logger)
    {
      _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var _token = Environment.GetEnvironmentVariable("NOODLE_CLIENT_TOKEN", EnvironmentVariableTarget.User);

      _client = new DiscordSocketClient();
      _client.Log += LogManager.LogAsync;

      await _client.LoginAsync(TokenType.Bot, _token);
      await _client.StartAsync();
      await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private IServiceProvider CreateProvider()
    {
      return new ServiceCollection()
        .AddSingleton(new DiscordSocketConfig()
        {

        })
        .AddSingleton<DiscordSocketClient>()
        .BuildServiceProvider();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_client == null)
        return;
      await _client.StopAsync();
    }
  }
}
