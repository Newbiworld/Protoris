using Azure;
using Discord;
using Discord.WebSocket;
using Protoris.Data;
using Protoris.Enum;
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
        private readonly DiscordSocketClient _client;

        public MusicService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava, DiscordSocketClient client)
        {
            _lava = lava;
            _client = client;
            _lava.OnTrackEnd += OnEndOfSong;
            _lava.OnTrackStuck += OnSongStuck;
            _lava.OnTrackException += OnTrackException;
        }

        #region Commands
        public async Task<ComponentBuilderV2> AddMusic(IVoiceChannel currentChannel,
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
                await _lava.DestroyPlayerAsync(currentChannel.GuildId);
                lavaPlayer = await _lava.JoinAsync(currentChannel);
            }

            if (!_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? musicPlaylist))
            {
                musicPlaylist = new MusicPlaylist()
                {
                    GuildId = guildId,
                    OriginationMessageChannel = currentMessageChannel,
                };

                _currentPlaylists.Add(guildId, musicPlaylist);
            }

            musicPlaylist.CurrentVoiceChannel = currentChannel;
            return await EnqueueMusic(guildId, musicPlaylist, lavaPlayer, requestedByUser, url);
        }

        public async Task Stop(ulong guildId, IVoiceChannel currentChannel)
        {
            LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);

            if (player != null && player.Track != null)
            {
                await LeaveAndCleanUp(guildId, player);
            }
        }

        public async Task Skip(ulong guildId)
        {
            LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);
            if (player != null)
            {
                await player.SeekAsync(_lava, player.Track.Duration);
            }
        }

        public async Task<ComponentBuilderV2> ShowPlaylist(ulong guildId, IGuildUser requestedBy)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                return await DiscordComponentHelper.BuildPlaylistResponse(requestedBy, guildPlayList.Playlist, _client);
            }

            return await DiscordComponentHelper.BuildPlaylistResponse(requestedBy, new List<TrackInformations>(), _client);
        }

        public async Task<ComponentBuilderV2> ShowGoTo(ulong guildId, IGuildUser requestedBy)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                return await DiscordComponentHelper.BuildGoToResponse(requestedBy, guildPlayList.Playlist, _client);
            }

            return await DiscordComponentHelper.BuildGoToResponse(requestedBy, new List<TrackInformations>(), _client);
        }

        public void RemoveSong(ulong guildId, IGuildUser requestedBy, string trackId)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                TrackInformations? trackToRemove = guildPlayList.Playlist.FirstOrDefault(x => x.Id == trackId);

                if (trackToRemove != null)
                {
                    guildPlayList.Playlist.Remove(trackToRemove);
                }
            }
        }

        public async Task GoToSong(ulong guildId, IGuildUser requestedBy, string trackId)
        {
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

                    await Skip(guildId);
                }
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

                    ComponentBuilderV2 builder = await DiscordComponentHelper.BuildPlayingTrackResponse(nextTrackInfo, _client);
                    await guildPlaylist.OriginationMessageChannel.SendMessageAsync(components: builder.Build());
                }
                else
                {
                    await LeaveAndCleanUp(guildId, player);
                }
            }
        }

        private async Task LeaveAndCleanUp(ulong guildId, LavaPlayer<LavaTrack> player)
        {
            if (_currentPlaylists.TryGetValue(guildId, out var playlist))
            {
                _currentPlaylists.Remove(guildId);

                if (player.Track != null)
                {
                    await player.SeekAsync(_lava, player.Track.Duration);
                }

                await _lava.LeaveAsync(playlist.CurrentVoiceChannel);
                await _lava.DestroyPlayerAsync(guildId);
            }
        }

        private async Task<ComponentBuilderV2> EnqueueMusic(ulong guildId,
            MusicPlaylist guildPlayList,
            LavaPlayer<LavaTrack> lavaPlayer,
            IGuildUser requestedBy,
            string url)
        {
            SearchResponse searchResponse = await SearchMusic(url);

            if (searchResponse.Type == SearchType.Empty || searchResponse.Type == SearchType.Error)
            {
                // Edge case: user send not a valid song at the start of everything
                if (lavaPlayer.Track == null)
                {
                    await LeaveAndCleanUp(guildId, lavaPlayer);
                }

                return await DiscordComponentHelper.BuildTrackNotFoundResponse(requestedBy, url, _client);
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
                return await DiscordComponentHelper.BuildAddingTrackResponse(trackInfo, _client);
            }
            else
            {
                await lavaPlayer.PlayAsync(_lava, trackInfo.Track);
                return await DiscordComponentHelper.BuildPlayingTrackResponse(trackInfo, _client);
            }
        }

        private async Task<SearchResponse> SearchMusic(string url)
        {
            return await _lava.LoadTrackAsync(url);
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
