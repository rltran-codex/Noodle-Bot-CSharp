using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace NoodleBotCSharp.Services.Commands.EchoCommand
{
  public class ModModule : InteractionModuleBase<SocketInteractionContext>
  {
    [SlashCommand("clear_chat", "Attempt to delete all messages in current channel")]
    public async Task ClearMessages()
    {
      var _user = ValidateAdminRole();
      if (_user == null)
      {
        return;
      }

      Stopwatch stopwatch = Stopwatch.StartNew();
      var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();

      foreach (var msg in messages.Skip(1))
      {
        try
        {
          await msg.DeleteAsync();
        }
        catch (Exception ex)
        {
          Log.Error(ex, "Error occurred trying to delete messages");
        }
      }

      // Stop the stopwatch
      stopwatch.Stop();

      Log.Information($"Purge completed in {Context.Channel.Name} by {_user.DisplayName}. Time taken: {stopwatch.Elapsed.TotalMilliseconds} ms");
      await FollowupAsync($"Purged messages in {Context.Channel.Name}. Committed by {_user.Mention}");
    }

    private EmbedBuilder NonAdminEmbed()
    {
      return new EmbedBuilder()
        .WithTitle("Insufficient Privileges")
        .WithImageUrl("https://i.redd.it/mu5d7u9uu1491.jpg")
        .WithColor(Color.DarkPurple);
    }

    private SocketGuildUser ValidateAdminRole()
    {
      DeferAsync();
      SocketGuildUser user = (SocketGuildUser)Context.User;
      if (!user.GuildPermissions.Administrator)
      {
        FollowupAsync(embed: NonAdminEmbed().Build());
        return null;
      }

      return user;
    }
  }
}