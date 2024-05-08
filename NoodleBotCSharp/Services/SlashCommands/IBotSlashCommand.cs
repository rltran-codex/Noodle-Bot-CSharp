using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace NoodleBotCSharp.Services.SlashCommands
{
  public interface IBotSlashCommand
  {
    SlashCommandBuilder GetSlashCommandBuilder();
    Task SlashCommandHandler(SocketSlashCommand command);
  }
}