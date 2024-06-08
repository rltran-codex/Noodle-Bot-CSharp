namespace Lavalink4NET.Discord_NET.ExampleBot;

using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;

/// <summary>
///     Template to develop Lavalink player for .NET using Lavalink4NET.
///     
///     This was taken from:
///     https://github.com/angelobreuer/Lavalink4NET/blob/dev/samples/Lavalink4NET.Discord_NET.ExampleBot/MusicModule.cs
/// </summary>
[RequireContext(ContextType.Guild)]
public sealed class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IAudioService _audioService;
  private VoteLavalinkPlayer currentPlayer;
  /// <summary>
  ///     Initializes a new instance of the <see cref="MusicModule"/> class.
  /// </summary>
  /// <param name="audioService">the audio service</param>
  /// <exception cref="ArgumentNullException">
  ///     thrown if the specified <paramref name="audioService"/> is <see langword="null"/>.
  /// </exception>
  public MusicModule(IAudioService audioService)
  {
    ArgumentNullException.ThrowIfNull(audioService);
    _audioService = audioService;
    _audioService.StartAsync();
    currentPlayer = null;
  }

  [SlashCommand("disconnect", "Disconnects from the current voice channel connected to", runMode: RunMode.Async)]
  public async Task Disconnect()
  {

  }

  [SlashCommand("play", description: "Plays music", runMode: RunMode.Async)]
  public async Task Play(string query)
  {
    await DeferAsync().ConfigureAwait(false);
    // Retrieve the player using the method we created earlier.
    // We allow to connect to the voice channel if the user is not connected.
    var player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

    // If the player is null, something failed. We already sent an error message to the user
    if (player is null)
    {
      return;
    }

    // Load the track from YouTube. This may take some time, so we await the result.
    var track = await _audioService.Tracks
        .LoadTrackAsync(query, TrackSearchMode.YouTube)
        .ConfigureAwait(false);

    var respEmbed = new EmbedBuilder();
    // If no track was found, we send an error message to the user.
    if (track is null)
    {
      respEmbed = new EmbedBuilder()
        .WithColor(Color.Red)
        .WithAuthor(Context.Interaction.User.Username, Context.Interaction.User.GetDisplayAvatarUrl() ??  Context.Interaction.User.GetAvatarUrl())
        .WithTitle("😖 No results found.")
        .WithDescription($"Unable to find <b>{query}</b>");
      await FollowupAsync(embed: respEmbed.Build()).ConfigureAwait(false);
      return;
    }

    // Play the track and inform the user about the track that is being played.
    await player.PlayAsync(track).ConfigureAwait(false);
    await FollowupAsync($"🔈 Playing: {track.Uri}").ConfigureAwait(false);
  }

  [SlashCommand("position", description: "Shows the track position", runMode: RunMode.Async)]
  public async Task Position()
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  [SlashCommand("stop", description: "Stops the current track", runMode: RunMode.Async)]
  public async Task Stop()
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  [SlashCommand("volume", description: "Sets the player volume (0 - 1000%)", runMode: RunMode.Async)]
  public async Task Volume(int volume = 100)
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  [SlashCommand("skip", description: "Skips the current track", runMode: RunMode.Async)]
  public async Task Skip()
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  [SlashCommand("pause", description: "Pauses the player.", runMode: RunMode.Async)]
  public async Task PauseAsync()
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  [SlashCommand("resume", description: "Resumes the player.", runMode: RunMode.Async)]
  public async Task ResumeAsync()
  {
    await RespondAsync("Sorry this is not yet supported...");
  }

  private async ValueTask<VoteLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
  {
    var retrieveOptions = new PlayerRetrieveOptions(
        ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

    var result = await _audioService.Players
        .RetrieveAsync(Context, playerFactory: PlayerFactory.Vote, retrieveOptions)
        .ConfigureAwait(false);

    if (!result.IsSuccess)
    {
      var errorMessage = result.Status switch
      {
        PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
        PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
        _ => "Unknown error.",
      };

      await FollowupAsync(errorMessage).ConfigureAwait(false);
      return null;
    }

    await result.Player.SetVolumeAsync(50 / 100f).ConfigureAwait(false);
    return result.Player;
  }
}