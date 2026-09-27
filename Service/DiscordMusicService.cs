using Discord;
using Discord.WebSocket;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Service.Interfaces;
using Protoris.Service.TrackResolver;

namespace Protoris.Service
{
    public class DiscordMusicService : IDiscordMusicService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IMusicPlaylistService _musicPlaylistService;
        private readonly IExceptionService _exceptionService;
        private readonly IMusicComponentService _musicComponentService;
        private readonly IDiscordContextProvider _discordContextProvider;
        private readonly IMasterTrackResolver _masterTrackResolver;
        private readonly ITrackPreloader _trackPreloader;

        public DiscordMusicService(
            DiscordSocketClient discordClient,
            IMusicPlaylistService musicPlaylistService,
            IExceptionService exceptionService,
            IMusicComponentService musicComponentService,
            IDiscordContextProvider discordContextProvider,
            IMasterTrackResolver masterTrackResolver,
            ITrackPreloader trackPreloader)
        {
            _discordClient = discordClient;
            _musicPlaylistService = musicPlaylistService;
            _exceptionService = exceptionService;
            _musicComponentService = musicComponentService;
            _discordContextProvider = discordContextProvider;
            _masterTrackResolver = masterTrackResolver;
            _trackPreloader = trackPreloader;
        }

        #region General Interaction
        public async Task PlayMusic(IInteractionContext context, string url)
        {
            try
            {
                IGuildUser? user = context.Interaction.User as IGuildUser;
                (IVoiceChannel? voiceChannel, string reason) = await IsValidUser(user);

                if (voiceChannel == null)
                {
                    await context.Interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                await context.Interaction.DeferAsync(); // In the off chance the loading takes more than 3 seconds, we defers it

                TrackInformations? trackInfo = await _masterTrackResolver.ResolveTrack(url, user!);
                trackInfo = await _trackPreloader.PreloadTrack(user!.Guild.Id, trackInfo);

                if (trackInfo == null)
                {
                    // We rather return here so we don't create context for everything!
                    await SendTracksNotFoundResponse(context, user!, url);
                    return;
                }

                if (!_discordContextProvider.TryGetContext(user!.Guild.Id, out DiscordContext discordContext))
                {
                    discordContext = await _discordContextProvider.CreateDiscordContext(context, voiceChannel!);
                }

                discordContext.VoiceChannel = voiceChannel!;

                EPlayMusicResult result = await _musicPlaylistService.AddMusic(discordContext, trackInfo);
                await SendPlayMusicResponse(context, result, discordContext, trackInfo);
            }
            catch (Exception exception)
            {
                await context.Interaction.RespondAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task MultiplePlayMusic(IInteractionContext context, string url)
        {
            try
            {
                IGuildUser? user = context.Interaction.User as IGuildUser;
                (IVoiceChannel? voiceChannel, string reason) = await IsValidUser(user);

                if (voiceChannel == null)
                {
                    await context.Interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                await context.Interaction.DeferAsync(); // In the off chance the loading takes more than 3 seconds, we defers it

                PlaylistInformations? playlistInfo = await _masterTrackResolver.ResolvePlaylist(url, user!);
                playlistInfo = await _trackPreloader.PreloadMultipleTracks(user!.Guild.Id, playlistInfo);

                if (playlistInfo == null || playlistInfo.PlaylistTracksInfo.Count <= 0)
                {
                    // We rather return here so we don't create context for everything!
                    await SendTracksNotFoundResponse(context, user!, url);
                    return;
                }

                if (!_discordContextProvider.TryGetContext(user!.Guild.Id, out DiscordContext discordContext))
                {
                    discordContext = await _discordContextProvider.CreateDiscordContext(context, voiceChannel!);
                }

                discordContext.VoiceChannel = voiceChannel!;
                EPlayMusicResult result = await _musicPlaylistService.AddMultipleMusic(discordContext, playlistInfo);
                await SendPlayMultipleMusicResponse(context, result, discordContext, playlistInfo, url);
            }
            catch (Exception exception)
            {
                await context.Interaction.RespondAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }
        }


        public async Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand commandEnum)
        {
            try
            {
                IGuildUser? guildUser = interaction.User as IGuildUser;
                (IVoiceChannel? voiceChannel, string reason) = await IsValidUser(guildUser);
                if (voiceChannel == null)
                {
                    await interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                ulong guildId = voiceChannel!.GuildId;

                switch (commandEnum)
                {
                    case EMusicCommand.Stop:
                        await SendStopResponse(guildId, interaction, guildUser!);
                        break;
                    case EMusicCommand.Skip:
                        await SendSkipResponse(guildId, interaction, guildUser!);
                        break;
                    case EMusicCommand.ShowPlaylist:
                        await SendPlaylistResponse(guildId, interaction, guildUser!);
                        break;
                    case EMusicCommand.ShowGoTo:
                        await SendGoToResponse(guildId, interaction, guildUser!);
                        break;
                }
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Can't stop, won't stop!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task UpdateCurrentlyPlayingSongs()
        {
            List<DiscordContext> allContext = _discordContextProvider.GetAllContext();
            
            foreach(DiscordContext context in allContext)
            {
                try
                {
                    if (context.CurrentPlayingMusic != null)
                    {
                        CurrentPlayingMusic currentPlayingMusic = context.CurrentPlayingMusic;
                        ComponentBuilderV2 builder = await _musicComponentService.BuildPlayingTrackResponse(context.BotUser, currentPlayingMusic.TrackInfo, currentPlayingMusic.GetCurrentTime());
                        await context.CurrentPlayingMusic.OriginalMessage.ModifyAsync(props => props.Components = builder.Build());
                    }
                }
                catch (Exception)
                {
                    // Swallow all exceptions, because this is a race conditions :3
                }
            }
        }

        public async Task CleanUpAndFarewell(ulong guildId)
        {
            DiscordContext? context = await CleanUp(guildId);

            if (context != null)
            {
                ComponentBuilderV2 component = await _musicComponentService.BuildFarewellResponse(context.BotUser);
                await context.CurrentMessageChannel.SendMessageAsync(components: component.Build());
            }
        }

        public async Task PlayNextSong(ulong guildId, IDiscordInteraction? interaction = null)
        {
            if (!_discordContextProvider.TryGetContext(guildId, out DiscordContext context))
                return;

            TrackInformations? info = await _musicPlaylistService.PlayNext(guildId);

            if (info != null)
            {
                ComponentBuilderV2 playBuilder = await _musicComponentService.BuildPlayingTrackResponse(context.BotUser, info, TimeSpan.Zero);
                IUserMessage messageSent = interaction != null
                    ? await interaction.FollowupAsync(components: playBuilder.Build())
                    : await context.CurrentMessageChannel.SendMessageAsync(components: playBuilder.Build());

                await context.RotatePlayingMusic(info, messageSent);
            }

        }

        public void StartTimerForCurrentSong(ulong guildId)
        {
            if (_discordContextProvider.TryGetContext(guildId, out DiscordContext context))
            {
                context.StartMusicTimer();
            }
        }

        #endregion

        #region  Component Interaction
        public async Task HandleComponentInteraction(IComponentInteraction interaction, string param, EMusicInteraction musicInteraction)
        {
            try
            {
                IGuildUser? guildUser = interaction.User as IGuildUser;
                (IVoiceChannel? voiceChannel, string reason) = await IsValidUser(guildUser);

                if (voiceChannel == null)
                {
                    await interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                AudioPlayerState response = await _musicPlaylistService.GetAudioPlayerState(guildUser!.Guild.Id);

                if (!response.IsPlaying)
                {
                    await interaction.RespondAsync(response.Reason, ephemeral: true);
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
            catch (Exception exception)
            {
                await interaction.RespondAsync("NOTHING SHALL BREAK ME, BBY!");
                _exceptionService.LogException(exception);
            }
        }

        private async Task RemoveSong(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ulong guildId = interaction.GuildId!.Value;
            await _musicPlaylistService.RemoveSong(guildId, trackId);
            await UpdatePlayListResponse(guildId, interaction, guildUser);
        }

        private async Task GoToTrack(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ulong guildId = interaction.GuildId!.Value;
            await _musicPlaylistService.GoToSong(guildId, trackId);
            await UpdateGoToResponse(guildId, interaction, guildUser, fixUglyNotskippingFirstSongBug: true);
        }

        private async Task ShowPlaylistInteraction(IComponentInteraction interaction, IGuildUser guildUser, string param)
        {
            int index = ExtractIndex(param, out bool shouldInit);
            if (shouldInit) await SendPlaylistResponse(interaction.GuildId!.Value, interaction, guildUser, index);
            else await UpdatePlayListResponse(interaction.GuildId!.Value, interaction, guildUser, index);
        }

        private async Task ShowGotoInteraction(IComponentInteraction interaction, IGuildUser guildUser, string param)
        {
            int index = ExtractIndex(param, out bool shouldInit);
            if (shouldInit) await SendGoToResponse(interaction.GuildId!.Value, interaction, guildUser, index);
            else await UpdateGoToResponse(interaction.GuildId!.Value, interaction, guildUser, index);
        }
        #endregion

        #region Validation
        private async Task<(IVoiceChannel? voiceChannel, string nullReason)> IsValidUser(IGuildUser? guildUser)
        {
            if (guildUser == null)
            {
                return (null, "I don't know why, but you don't exist?");
            }

            if (guildUser.VoiceChannel == null)
            {
                return (null, "I don't listen to people who ain't with me in vc!");
            }

            return (guildUser.VoiceChannel, string.Empty);
        }
        #endregion

        #region Helper
        public async Task<IUserMessage> HandleResponse(IDiscordInteraction interaction, ComponentBuilderV2 builder, bool ephemeral = false)
        {
            await interaction.RespondAsync(components: builder.Build(), ephemeral: ephemeral);
            return await interaction.GetOriginalResponseAsync();
        }

        public async Task HandleUpdateResponse(IComponentInteraction interaction, ComponentBuilderV2 builder)
        {
            await interaction.UpdateAsync(m => m.Components = builder.Build());
        }

        public async Task<IUserMessage> HandleFollowUp(IDiscordInteraction interaction, ComponentBuilderV2 builder, bool ephemeral = false)
        {
            return await interaction.FollowupAsync(components: builder.Build(), ephemeral: ephemeral);
        }

        private int ExtractIndex(string param, out bool shouldInit)
        {
            shouldInit = param.Contains("init");
            if (shouldInit) return 0;
            return int.Parse(param);
        }

        public async Task SendPlaylistResponse(ulong guildId, IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0)
        {
            if (_musicPlaylistService.TryGetPlaylist(guildId, out IReadOnlyCollection<TrackInformations> playlist) && _discordContextProvider.TryGetContext(guildId, out DiscordContext context))
            {
                ComponentBuilderV2 builder = await _musicComponentService.BuildPlaylistResponse(context.BotUser, requestedBy, playlist, index);
                IUserMessage message = await HandleResponse(interaction, builder, true);
                context.MessageHistory.Add(message);
            }
            else
            {
                // Error handler Here
            }
        }

        private async Task UpdatePlayListResponse(ulong guildId, IComponentInteraction interaction, IGuildUser requestedBy, int index = 0)
        {

            if (_musicPlaylistService.TryGetPlaylist(guildId, out IReadOnlyCollection<TrackInformations> playlist) && _discordContextProvider.TryGetContext(guildId, out DiscordContext discordContext))
            {
                ComponentBuilderV2 builder = await _musicComponentService.BuildPlaylistResponse(discordContext.BotUser, requestedBy, playlist, index);
                await HandleUpdateResponse(interaction, builder);
            }
            else
            {
                // Error handler Here
            }
        }

        public async Task SendGoToResponse(ulong guildId, IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0)
        {
            if (_musicPlaylistService.TryGetPlaylist(guildId, out IReadOnlyCollection<TrackInformations> playlist) && _discordContextProvider.TryGetContext(guildId, out DiscordContext context))
            {
                ComponentBuilderV2 builder = await _musicComponentService.BuildGoToResponse(context.BotUser, requestedBy, playlist, index);
                IUserMessage message = await HandleResponse(interaction, builder, true);
                context.MessageHistory.Add(message);
            }
            else
            {
                // Error handler Here
            }
        }

        private async Task UpdateGoToResponse(ulong guildId, IComponentInteraction interaction, IGuildUser requestedBy, int index = 0, bool fixUglyNotskippingFirstSongBug = false)
        {

            if (_musicPlaylistService.TryGetPlaylist(guildId, out IReadOnlyCollection<TrackInformations> playlist) && _discordContextProvider.TryGetContext(guildId, out DiscordContext discordContext))
            {
                if (fixUglyNotskippingFirstSongBug) playlist = playlist.Skip(1).ToList();
                ComponentBuilderV2 builder = await _musicComponentService.BuildGoToResponse(discordContext.BotUser, requestedBy, playlist, index);
                await HandleUpdateResponse(interaction, builder);
            }
            else
            {
                // Error handler Here
            }
        }

        private async Task SendStopResponse(ulong guildId, IDiscordInteraction interaction, IGuildUser requestedBy)
        {
            if (_discordContextProvider.TryGetContext(guildId, out DiscordContext discordContext))
            {
                await _musicPlaylistService.Stop(guildId);
                ComponentBuilderV2 component = await _musicComponentService.BuildStopResponse(discordContext.BotUser, requestedBy);
                await HandleResponse(interaction, component);
            }
            else
            {
                // Error handler Here
            }
        }

        private async Task SendSkipResponse(ulong guildId, IDiscordInteraction interaction, IGuildUser requestedBy)
        {
            if (_discordContextProvider.TryGetContext(guildId, out DiscordContext discordContext))
            {
                await _musicPlaylistService.Skip(guildId);
                ComponentBuilderV2 component = await _musicComponentService.BuildSkipResponse(discordContext.BotUser, requestedBy);
                await HandleResponse(interaction, component);
            }
            else
            {
                // Error handler Here
            }
        }

        public async Task<DiscordContext?> CleanUp(ulong guildId)
        {
            IVoiceChannel? connectedVoiceChannel = _discordClient.GetGuild(guildId)?.GetUser(_discordClient.CurrentUser.Id)?.VoiceChannel;

            if (_discordContextProvider.TryGetContext(guildId, out DiscordContext context))
            {
                await context.CleanUp();
                _discordContextProvider.Remove(guildId);
            }

            await _musicPlaylistService.CleanUp(guildId, context?.BotUser?.VoiceChannel ?? connectedVoiceChannel);

            return context;
        }

        private async Task SendPlayMusicResponse(IInteractionContext context,
            EPlayMusicResult playMusicResult,
            DiscordContext discordContext,
            TrackInformations trackInfo)
        {
            ComponentBuilderV2 builder = await _musicComponentService.BuildAddingTrackResponse(discordContext.BotUser, trackInfo);
            IUserMessage createdMessage = await HandleFollowUp(context.Interaction, builder);
            discordContext.MessageHistory.Add(createdMessage);
            
            if (playMusicResult == EPlayMusicResult.NextToPlay)
            {
                await PlayNextSong(discordContext.GuildId, context.Interaction);
            }
        }

        private async Task SendPlayMultipleMusicResponse(IInteractionContext context,
            EPlayMusicResult playMusicResult,
            DiscordContext discordContext,
            PlaylistInformations playlistToAdd,
            string url)
        {
            ComponentBuilderV2 builder = await _musicComponentService.BuildAddingTracksResponse(discordContext.BotUser, playlistToAdd);
            IUserMessage createdMessage = await HandleFollowUp(context.Interaction, builder);
            discordContext.MessageHistory.Add(createdMessage);

            if (playMusicResult == EPlayMusicResult.NextToPlay)
            {
                await PlayNextSong(discordContext.GuildId, context.Interaction);
            }
        }

        private async Task SendTracksNotFoundResponse(IInteractionContext context, 
            IGuildUser requestedBy,
            string url)
        {
            IGuildUser botUser = await context.Guild.GetCurrentUserAsync();
            ComponentBuilderV2 builder = await _musicComponentService.BuildTrackNotFoundResponse(botUser, requestedBy, url);
            IUserMessage createdMessage = await HandleFollowUp(context.Interaction, builder);
            if (_discordContextProvider.TryGetContext(botUser.GuildId, out DiscordContext discordContext))
                discordContext.MessageHistory.Add(createdMessage);
        }
        #endregion
    }
}
