
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace NoodleBotCSharp.Services.Commands.EchoCommand
{
  public class EchoModule : InteractionModuleBase<SocketInteractionContext>
  {
    private readonly string commandName = "echo";

    [SlashCommand("echo", "Echo an input")]
    public async Task Echo(string input)
    {
      IUser sentBy = this.Context.User;
      await RespondAsync($"{sentBy.Mention}: {input}");
    }

    [SlashCommand("echo-fancy", "Echo an input with an embed")]
    public async Task EchoEmbed(string input)
    {
      IUser sentBy = Context.User;
      var embed = new EmbedBuilder()
        .WithAuthor(sentBy.Username, sentBy.GetAvatarUrl() ?? sentBy.GetDefaultAvatarUrl())
        .WithColor(Color.LightOrange)
        .WithTitle(commandName)
        .WithDescription(input);

      await RespondAsync(embed: embed.Build());
    }
  }
}