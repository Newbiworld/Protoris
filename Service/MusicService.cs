using Discord;
using Protoris.Data;
using Protoris.Service.Interfaces;
using Victoria;
using Victoria.Rest.Search;
using Victoria.WebSocket.EventArgs;

namespace Protoris.Service
{
    public class MusicService : IMusicService
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly Dictionary<ulong, MusicPlaylist> _currentPlaylists = new Dictionary<ulong, MusicPlaylist>();
        private readonly Dictionary<ulong, CurrentPlayingMusic> _currentlyPlayingTrack = 
            new Dictionary<ulong, CurrentPlayingMusic>();
        private readonly IMusicComponentService _musicComponentService;

        public MusicService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava, IMusicComponentService musicComponentService)
        {
            _lava = lava;
            _lava.OnTrackEnd += OnEndOfSong;
            _lava.OnTrackStuck += OnSongStuck;
            _lava.OnTrackException += OnTrackException;
            _musicComponentService = musicComponentService;
        }

        #region Commands
        public async Task AddMusic(IInteractionContext context,
            IVoiceChannel currentChannel,
            IMessageChannel currentMessageChannel,
            IGuildUser requestedByUser,
            string url)
        {
            ulong guildId = currentChannel.GuildId;
            LavaPlayer<LavaTrack>? lavaPlayer = await _lava.TryGetPlayerAsync(guildId);

            // If null, the player don't exist, so the bot is not in a vc in the guild
            if (lavaPlayer == null || !lavaPlayer.State.IsConnected)
            {
                lavaPlayer = await _lava.JoinAsync(currentChannel);
            }
            else if (!lavaPlayer.State.IsConnected)
            {
                // Can't be sure of the quality of the bot
                await _lava.DestroyPlayerAsync(guildId);
                lavaPlayer = await _lava.JoinAsync(currentChannel);
            }

            MusicPlaylist musicPlaylist = await InitializePlaylist(currentChannel, currentMessageChannel, context, lavaPlayer);
            await EnqueueMusic(guildId, context.Interaction, musicPlaylist, lavaPlayer, requestedByUser, url);
        }

        public async Task AddMusics(IInteractionContext context,
            IVoiceChannel currentChannel,
            IMessageChannel currentMessageChannel,
            IGuildUser requestedByUser,
            PlaylistToAddInfo playlistToAdd)
        {
            ulong guildId = currentChannel.GuildId;
            LavaPlayer<LavaTrack>? lavaPlayer = await _lava.TryGetPlayerAsync(guildId);

            // If null, the player don't exist, so the bot is not in a vc in the guild
            if (lavaPlayer == null || !lavaPlayer.State.IsConnected)
            {
                lavaPlayer = await _lava.JoinAsync(currentChannel);
            }
            else if (!lavaPlayer.State.IsConnected)
            {
                // Can't be sure of the quality of the bot
                await _lava.DestroyPlayerAsync(guildId);
                lavaPlayer = await _lava.JoinAsync(currentChannel);
            }

            MusicPlaylist musicPlaylist = await InitializePlaylist(currentChannel, currentMessageChannel, context, lavaPlayer);
            await EnqueueMusics(guildId, context.Interaction, musicPlaylist, lavaPlayer, requestedByUser, playlistToAdd);
        }

        public async Task Stop(IVoiceChannel currentChannel, IGuildUser requestedBy, IDiscordInteraction? interaction = null)
        {
            ulong guildId = requestedBy.GuildId;
            LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);

            if (player != null && player.Track != null)
            {
                await LeaveAndCleanUp(guildId, player);
            }
            else if (interaction != null)
            {
                MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
                // Don't send message when it's killed manually
                ComponentBuilderV2 component = await _musicComponentService.BuildStopResponse(guildPlayList.BotUser, requestedBy);
                await HandleResponse(interaction, component);
            }
        }

        public async Task Skip(IDiscordInteraction interaction, IGuildUser requestedBy)
        {
            ulong guildId = requestedBy.GuildId;

            LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);
            if (player != null)
            {
                await player.SeekAsync(_lava, player.Track.Duration);
            }

            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
            ComponentBuilderV2 component = await _musicComponentService.BuildSkipResponse(guildPlayList.BotUser, requestedBy);
            await HandleResponse(interaction, component);       
        }

        public async Task ShowPlaylist(IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0)
        {
            ulong guildId = requestedBy.GuildId;
            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
            ComponentBuilderV2 builder = await _musicComponentService.BuildPlaylistResponse(guildPlayList.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), index);
            await HandleResponse(interaction, builder, true);
        }

        public async Task ShowGoTo(IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0)
        {
            ulong guildId = requestedBy.GuildId;
            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
            ComponentBuilderV2 builder = await _musicComponentService.BuildGoToResponse(guildPlayList.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), index);
            await HandleResponse(interaction, builder, true);
        }

        public async Task<ComponentBuilderV2> ShowGoTo(IGuildUser requestedBy, int index = 0)
        {
            ulong guildId = requestedBy.GuildId;
            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
            return await _musicComponentService.BuildGoToResponse(guildPlayList.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), index);
        }

        public async Task<ComponentBuilderV2> ShowPlaylist(IGuildUser requestedBy, int index = 0)
        {
            ulong guildId = requestedBy.GuildId;
            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);
            return await _musicComponentService.BuildPlaylistResponse(guildPlayList.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), index);
        }

        public async Task<ComponentBuilderV2> RemoveSong(IGuildUser requestedBy, string trackId)
        {
            if (_currentPlaylists.TryGetValue(requestedBy.GuildId, out MusicPlaylist? guildPlayList))
            {
                TrackInformations? trackToRemove = guildPlayList.Playlist.FirstOrDefault(x => x.Id == trackId);

                if (trackToRemove != null)
                {
                    guildPlayList.Playlist.Remove(trackToRemove);
                }
            }

            return await _musicComponentService.BuildPlaylistResponse(guildPlayList.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), 0);
        }

        public async Task<ComponentBuilderV2> GoToSong(IGuildUser requestedBy, string trackId)
        {
            ulong guildId = requestedBy.GuildId;
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                TrackInformations? trackToGoTo = guildPlayList.Playlist.FirstOrDefault(x => x.Id == trackId);

                if (trackToGoTo != null)
                {
                    int numberToRemove = guildPlayList.Playlist.IndexOf(trackToGoTo);

                    if (numberToRemove > 0)
                    {
                        guildPlayList.Playlist.RemoveRange(0, numberToRemove);
                    }

                    LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);

                    if (player != null)
                    {
                        await player.SeekAsync(_lava, player.Track.Duration);
                    }
                }
            }

            return await _musicComponentService.BuildPlaylistResponse(guildPlayList!.BotUser, requestedBy, guildPlayList?.Playlist ?? new List<TrackInformations>(), 0);
        }

        public async Task UpdateCurrentlyPlayingSongs()
        {
            try
            {
                foreach (CurrentPlayingMusic currentPlayingMusic in _currentlyPlayingTrack.Values)
                {
                    MusicPlaylist? playlist = _currentPlaylists.GetValueOrDefault(currentPlayingMusic.GuildId);
                    if (playlist != null)
                    {

                        ComponentBuilderV2 builder = await _musicComponentService.BuildPlayingTrackResponse(playlist.BotUser, currentPlayingMusic.TrackInfo, currentPlayingMusic.GetCurrentTime());
                        await currentPlayingMusic.OriginalMessage.ModifyAsync(props => props.Components = builder.Build());

                    }
                }
            }
            catch (Exception)
            {
                // Swallow all exceptions, because this is a race conditions :3
            }
        }
        #endregion

        #region Helper
        private async Task HandleMusicChange(ulong guildId)
        {
            LavaPlayer<LavaTrack> player = await _lava.TryGetPlayerAsync(guildId);

            if (player != null && _currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlaylist))
            {
                // Don't play if the bloke isn't connected
                if (guildPlaylist.Playlist.Any() && player.State.IsConnected)
                {
                    TrackInformations nextTrackInfo = guildPlaylist.Playlist.First();
                    guildPlaylist.Playlist.RemoveAt(0);
                    await player.PlayAsync(_lava, nextTrackInfo.Track);

                    ComponentBuilderV2 builder = await _musicComponentService.BuildPlayingTrackResponse(guildPlaylist.BotUser, nextTrackInfo, TimeSpan.Zero);
                    IUserMessage messageSent = await guildPlaylist.OriginationMessageChannel.SendMessageAsync(components: builder.Build());
                    await ChangeCurrentMusic(nextTrackInfo, messageSent, guildId);
                }
                else
                {
                    await LeaveAndCleanUp(guildId, player);
                }
            }
        }

        private async Task LeaveAndCleanUp(ulong guildId, LavaPlayer<LavaTrack> player)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? playlist))
            {
                _currentPlaylists.Remove(guildId);

                if (player.Track != null)
                {
                    await player.SeekAsync(_lava, player.Track.Duration);
                }

                await _lava.LeaveAsync(playlist.CurrentVoiceChannel);
                await _lava.DestroyPlayerAsync(guildId);

                foreach (IUserMessage message in playlist.SentMessages)
                {
                    try
                    {
                        await message.DeleteAsync();
                    }
                    catch (Exception)
                    {
                        //  Swallow all exceptions (again) since we're not sure which was erased
                    }
                }

                ComponentBuilderV2 component = await _musicComponentService.BuildFarewellResponse(playlist.BotUser);
                await playlist.OriginationMessageChannel.SendMessageAsync(components: component.Build());
            }

            if (_currentlyPlayingTrack.TryGetValue(guildId, out CurrentPlayingMusic? currentPlayingMusic))
            {
                await currentPlayingMusic.OriginalMessage.DeleteAsync();
                _currentlyPlayingTrack.Remove(guildId);
            }
        }

        private async Task EnqueueMusic(ulong guildId,
            IDiscordInteraction interaction,
            MusicPlaylist guildPlayList,
            LavaPlayer<LavaTrack> lavaPlayer,
            IGuildUser requestedBy,
            string url)
        {
            SearchResponse searchResponse = await _lava.LoadTrackAsync(url);

            if (searchResponse.Type == SearchType.Empty || searchResponse.Type == SearchType.Error)
            {
                // Edge case: user send not a valid song at the start of everything
                if (lavaPlayer.Track == null)
                {
                    await LeaveAndCleanUp(guildId, lavaPlayer);
                }

                ComponentBuilderV2 builder = await _musicComponentService.BuildTrackNotFoundResponse(guildPlayList.BotUser, requestedBy, url);
                IUserMessage createdMessage = await interaction.FollowupAsync(components: builder.Build());
                guildPlayList.SentMessages.Add(createdMessage);
                return;
            }

            TrackInformations trackInfo = new TrackInformations()
            {
                RequestedBy = requestedBy,
                Track = searchResponse.Tracks.First()
            };

            // Something is playing, that's buenos
            if (lavaPlayer.Track != null)
            {
                guildPlayList.Playlist.Add(trackInfo);
                ComponentBuilderV2 builder = await _musicComponentService.BuildAddingTrackResponse(guildPlayList.BotUser, trackInfo);
                IUserMessage createdMessage = await interaction.FollowupAsync(components: builder.Build());
                guildPlayList.SentMessages.Add(createdMessage);
            }
            else
            {
                await lavaPlayer.PlayAsync(_lava, trackInfo.Track);
                ComponentBuilderV2 builder = await _musicComponentService.BuildPlayingTrackResponse(guildPlayList.BotUser, trackInfo, TimeSpan.Zero);
                IUserMessage messageSent = await interaction.FollowupAsync(components: builder.Build());
                await ChangeCurrentMusic(trackInfo, messageSent, guildId);
            }
        }

        private async Task EnqueueMusics(ulong guildId,
            IDiscordInteraction interaction,
            MusicPlaylist guildPlayList,
            LavaPlayer<LavaTrack> lavaPlayer,
            IGuildUser requestedBy,
            PlaylistToAddInfo playlistToAdd)
        {
            foreach(string url in playlistToAdd.PlaylistVideosUrl)
            {
                SearchResponse searchResponse = await _lava.LoadTrackAsync(url);
                if (searchResponse.Type == SearchType.Empty || searchResponse.Type == SearchType.Error)
                {
                    continue;
                }

                playlistToAdd.PlaylistTracksInfo.Add(new TrackInformations()
                {
                    RequestedBy = requestedBy,
                    Track = searchResponse.Tracks.First()
                });
            }

            bool hasSentAnswer = false;
            if (lavaPlayer.Track == null && playlistToAdd.PlaylistTracksInfo.Count <= 0)
            {
                ComponentBuilderV2 earlyBuilder = await _musicComponentService.BuildAddingTracksResponse(guildPlayList.BotUser, playlistToAdd);
                await interaction.FollowupAsync(components: earlyBuilder.Build());
                await LeaveAndCleanUp(guildId, lavaPlayer);
                return;
            }

            // Nothing is playing, force the first one to play then add the rest
            else if (lavaPlayer.Track == null)
            {
                TrackInformations trackInfo = playlistToAdd.PlaylistTracksInfo[0];
                playlistToAdd.PlaylistTracksInfo.RemoveAt(0);

                await lavaPlayer.PlayAsync(_lava, trackInfo.Track);
                ComponentBuilderV2 playBuilder = await _musicComponentService.BuildPlayingTrackResponse(guildPlayList.BotUser, trackInfo, TimeSpan.Zero);
                IUserMessage messageSent = await interaction.FollowupAsync(components: playBuilder.Build());
                await ChangeCurrentMusic(trackInfo, messageSent, guildId);
                hasSentAnswer = true;
            }

            guildPlayList.Playlist.AddRange(playlistToAdd.PlaylistTracksInfo);
            ComponentBuilderV2 builder = await _musicComponentService.BuildAddingTracksResponse(guildPlayList.BotUser, playlistToAdd);
            IUserMessage createdMessage = hasSentAnswer
                ? await guildPlayList.OriginationMessageChannel.SendMessageAsync(components: builder.Build())
                : await interaction.FollowupAsync(components: builder.Build());

            guildPlayList.SentMessages.Add(createdMessage);
        }

        public async Task ChangeCurrentMusic(TrackInformations trackInfo, IUserMessage messageSent, ulong guildId)
        {
            CurrentPlayingMusic nextPlayingMusic = new CurrentPlayingMusic(trackInfo, messageSent, guildId);
            if (_currentlyPlayingTrack.TryGetValue(guildId, out CurrentPlayingMusic? currentPlayingMusic))
            {
                await currentPlayingMusic.OriginalMessage.DeleteAsync();
                _currentlyPlayingTrack.Remove(guildId);
            }

            _currentlyPlayingTrack.Add(guildId, nextPlayingMusic);
        }

        public async Task HandleResponse(IDiscordInteraction interaction, ComponentBuilderV2 builder, bool ephemeral = false)
        {
            await interaction.RespondAsync(components: builder.Build(), ephemeral: ephemeral);
            IUserMessage createdMessage = await interaction.GetOriginalResponseAsync();

            // Should never be null, since it would crash earlier, but as a safeguard
            if (interaction.GuildId != null)
            {
                ulong guildId = interaction.GuildId.Value;
                MusicPlaylist? guildPlaylist = _currentPlaylists.GetValueOrDefault(guildId);

                if (guildPlaylist != null)
                    guildPlaylist.SentMessages.Add(createdMessage);
            }
        }

        private async Task<MusicPlaylist> InitializePlaylist(IVoiceChannel currentChannel,
            IMessageChannel messageChannel,
            IInteractionContext context,
            LavaPlayer<LavaTrack>? lavaPlayer)
        {
            ulong guildId = currentChannel.GuildId;
   
            if (!_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? musicPlaylist))
            {
                IGuildUser botUser = await context.Guild.GetCurrentUserAsync();
                musicPlaylist = new MusicPlaylist()
                {
                    BotUser = botUser,
                    GuildId = guildId,
                    OriginationMessageChannel = messageChannel,
                };

                _currentPlaylists.Add(guildId, musicPlaylist);
            }

            musicPlaylist.CurrentVoiceChannel = currentChannel;
            return musicPlaylist;
        }
        #endregion

        #region Events
        private async Task OnEndOfSong(TrackEndEventArg eventArgument)
        {
            await HandleMusicChange(eventArgument.GuildId);
        }

        private async Task OnSongStuck(TrackStuckEventArg eventArgument)
        {
            await HandleMusicChange(eventArgument.GuildId);
        }

        private async Task OnTrackException(TrackExceptionEventArg eventArgument)
        {
            await HandleMusicChange(eventArgument.GuildId);
        }
        #endregion
    }
}
