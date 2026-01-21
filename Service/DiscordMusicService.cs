using Discord;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Service.Interfaces;
using Victoria;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;

namespace Protoris.Service
{
    public class DiscordMusicService : IDiscordMusicService
    {
        private readonly IMusicService _musicService;
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly IExceptionService _exceptionService;
        private readonly YoutubeClient _youtubeClient;
        public DiscordMusicService(
            IMusicService musicService,
            LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava,
            IExceptionService exceptionService)
        {
            _musicService = musicService;
            _exceptionService = exceptionService;
            _lava = lava;
            _youtubeClient = new YoutubeClient();
        }

        #region General Interaction
        public async Task PlayMusic(IInteractionContext context,
            IMessageChannel currentMessageChannel,
            string url)
        {
            try
            {
                IGuildUser? user = context.Interaction.User as IGuildUser;
                if (user == null)
                {
                    await context.Interaction.RespondAsync("You don't event exist!", ephemeral: true);
                    return;
                }

                IVoiceChannel? currentVoiceChannel = user.VoiceChannel;
                if (currentVoiceChannel == null)
                {
                    await context.Interaction.RespondAsync("Woah, I can't sing when you're not even in VC!", ephemeral: true);
                    return;
                }

                await context.Interaction.DeferAsync(); // In the off chance the loading takes more than 3 seconds, we defers it
                await _musicService.AddMusic(context, currentVoiceChannel, currentMessageChannel, user, url);
            }
            catch (Exception exception)
            {
                await context.Interaction.RespondAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task MultiplePlayMusic(IInteractionContext context, IMessageChannel currentMessageChannel, string url)
        {
            try
            {
                IGuildUser? user = context.Interaction.User as IGuildUser;
                if (user == null)
                {
                    await context.Interaction.RespondAsync("You don't event exist!", ephemeral: true);
                    return;
                }

                IVoiceChannel? currentVoiceChannel = user.VoiceChannel;
                if (currentVoiceChannel == null)
                {
                    await context.Interaction.RespondAsync("Woah, I ain't accepting this list when you're not with me!", ephemeral: true);
                    return;
                }

                await context.Interaction.DeferAsync(); // In the off chance the loading takes more than 3 seconds, we defers it
                PlaylistToAddInfo playlistInfo = await GetPlaylistInfoFromUrl(url);
                playlistInfo.RequestedBy = user;
                await _musicService.AddMusics(context, currentVoiceChannel, currentMessageChannel, user, playlistInfo);
            }
            catch (Exception exception)
            {
                await context.Interaction.RespondAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }
        }


        public async Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand commandEnum, int index = 0)
        {
            try
            {
                IGuildUser? guildUser = interaction.User as IGuildUser;
                (bool canApplyCommand, string reason) = await CanApplyCommand(guildUser, _lava);

                if (!canApplyCommand)
                {
                    await interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                switch (commandEnum)
                {
                    case EMusicCommand.Stop:
                        await _musicService.Stop(guildUser!.VoiceChannel, guildUser, interaction);
                        break;
                    case EMusicCommand.Skip:
                        await _musicService.Skip(interaction, guildUser!);
                        break;
                    case EMusicCommand.ShowPlaylist:
                        await _musicService.ShowPlaylist(interaction, guildUser!, index);
                        break;
                    case EMusicCommand.ShowGoTo:
                        await _musicService.ShowGoTo(interaction, guildUser!, index);
                        break;
                }
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Can't stop, won't stop!");
                _exceptionService.LogException(exception);
            }
        }
        #endregion

        #region  Component Interaction
        public async Task HandleComponentInteraction(IComponentInteraction interaction, string param, EMusicInteraction musicInteraction )
        {
            IGuildUser? guildUser = interaction.User as IGuildUser;
            (bool canApplyCommand, string reason) = await CanApplyCommand(guildUser, _lava);

            if (!canApplyCommand)
            {
                await interaction.RespondAsync(reason, ephemeral: true);
                return;
            }

            switch (musicInteraction)
            {
                case EMusicInteraction.Remove:
                    await RemoveSong(interaction, guildUser!, param);
                    break;
                case EMusicInteraction.GoToSong:
                    await GoToTrack(interaction, guildUser!, param);
                    break;
                case EMusicInteraction.Playlist:
                    await ShowPlaylistInteraction(interaction, guildUser!, param);
                    break;
                case EMusicInteraction.GoTo:
                    await ShowGotoInteraction(interaction, guildUser!, param);
                    break;
            }
        }

        private async Task RemoveSong(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ComponentBuilderV2 builder = await _musicService.RemoveSong(guildUser, trackId);
            await interaction.UpdateAsync(m => m.Components = builder.Build());
        }

        private async Task GoToTrack(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ComponentBuilderV2 builder = await _musicService.GoToSong(guildUser, trackId);
            await interaction.UpdateAsync(m => m.Components = builder.Build());
        }

        private async Task ShowPlaylistInteraction(IComponentInteraction interaction, IGuildUser guildUser, string param)
        {
            bool shouldInit = param.Contains("init");
            if (shouldInit) param.Replace("init", string.Empty);
            int.TryParse(param, out int index);

            if (shouldInit)
            {
                await _musicService.ShowPlaylist(interaction, guildUser!, index);
            }
            else
            {
                ComponentBuilderV2 builder = await _musicService.ShowPlaylist(guildUser, index);
                await interaction.UpdateAsync(m => m.Components = builder.Build());
            }
        }

        private async Task ShowGotoInteraction(IComponentInteraction interaction, IGuildUser guildUser, string param)
        {
            bool shouldInit = param.Contains("init");
            if (shouldInit) param.Replace("init", string.Empty);
            int.TryParse(param, out int index);

            if (shouldInit)
            {
                await _musicService.ShowGoTo(interaction, guildUser!, index);
            }
            else
            {
                ComponentBuilderV2 builder = await _musicService.ShowGoTo(guildUser, index);
                await interaction.UpdateAsync(m => m.Components = builder.Build());
            }
        }

        private async Task<PlaylistToAddInfo> GetPlaylistInfoFromUrl(string url)
        {
            PlaylistToAddInfo playlistToAddInfo = new PlaylistToAddInfo();
            playlistToAddInfo.PlaylistUrl = url;

            // Only has youtube for now
            try
            {
                if (url.Contains("youtube"))
                {
                    Playlist playlistMetadata = await _youtubeClient.Playlists.GetAsync(url);
                    IReadOnlyCollection<PlaylistVideo> playlistVideosMetadata = await _youtubeClient.Playlists.GetVideosAsync(url);
                    playlistToAddInfo.PlaylistName = playlistMetadata?.Title;
                    foreach (PlaylistVideo videoMetadata in playlistVideosMetadata)
                    {
                        playlistToAddInfo.PlaylistVideosUrl.Add(videoMetadata.Url);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing because we're cool and hips like that
            }


            return playlistToAddInfo;
        }
        #endregion

        #region Validation
        private async Task<(bool canApplyCommand, string reason)> CanApplyCommand(IGuildUser? guildUser, LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava)
        {
            if (guildUser == null)
            {
                return (false, "I don't know why, but you don't exist?");
            }

            if (guildUser.VoiceChannel == null)
            {
                return (false, "I don't listen to people who ain't with me in vc!");
            }

            LavaPlayer<LavaTrack>? lavaPlayer = await lava.TryGetPlayerAsync(guildUser.GuildId);
            if (lavaPlayer == null)
            {
                return (false, "Wow, don't tell me to do stuff when I ain't even on stage!");
            }

            if (lavaPlayer.Track == null)
            {
                return (false, "Wow, don't tell me what to do when I ain't singing! ... rude");
            }

            return (true, string.Empty);
        }
        #endregion
    }
}
