using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using NoodleBotCSharp.Services.SlashCommands;
using Serilog;

namespace NoodleBotCSharp.Services.SlashCommands
{
  public class EchoCommand : IBotSlashCommand
  {
    private const string commandName = "echo";
    private const string optionMessage = "message";

    public SlashCommandBuilder GetSlashCommandBuilder()
    {
      return new SlashCommandBuilder()
        .WithName(commandName)
        .WithDescription("Echo back the message provided.")
        .AddOption(
          optionMessage,
          ApplicationCommandOptionType.String,
          "The message to be echoed",
          isRequired: true
        );
    }

    public async Task SlashCommandHandler(SocketSlashCommand command)
    {
      SocketSlashCommandData d = command.Data;
      await command.RespondAsync(
        text: command.Data.Options
          .Where(o => o.Name == optionMessage)
          .First()
          .Value
          .ToString()
      );
    }
  }
}