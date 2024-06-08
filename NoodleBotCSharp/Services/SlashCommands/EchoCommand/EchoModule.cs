
using System;
using System.Linq;
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
      IUser sentBy = Context.User;
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

    [SlashCommand("repo", "Visit my source code and contribute.")]
    public async Task SendRepoLink()
    {
      IUser user = Context.User;
      IRole devs = Context.Guild.Roles.Where(x => x.Name == "App Dev").FirstOrDefault();
      var msg = $"{user.Mention}, please feel free to reach out to {devs.Mention} for help if you're interested in contributing.";
      var url = "https://github.com/rltran-codex/Noodle-Bot-CSharp";
      var title = url.Split("/").Last();
      var embed = new EmbedBuilder()
          .WithColor(Color.Teal)
          .WithAuthor(
            Context.Client.CurrentUser.Username, 
            Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl()
          ).WithUrl(url)
          .WithTitle($"GitHub: {title}")
          .WithDescription(msg);

      await RespondAsync(embed: embed.Build());
    }
  }
}