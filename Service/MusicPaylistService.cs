using Discord;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Service.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Protoris.Service
{
    public class MusicPaylistService : IMusicPlaylistService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly Dictionary<ulong, MusicPlaylist> _currentPlaylists = new Dictionary<ulong, MusicPlaylist>();

        public MusicPaylistService(IAudioPlayer audioPlayer)
        {
            _audioPlayer = audioPlayer;
        }

        #region Commands
        public async Task<EPlayMusicResult> AddMusic(DiscordContext discordContext, TrackInformations track)
        {
            ulong guildId = discordContext.GuildId;
            await _audioPlayer.JoinAsync(discordContext.VoiceChannel);
            MusicPlaylist musicPlaylist = InitializePlaylist(guildId);
            return await EnqueueTracks(guildId, musicPlaylist, [track]);
        }

        public async Task<EPlayMusicResult> AddMultipleMusic(DiscordContext discordContext, PlaylistInformations playlistToAdd)
        {
            ulong guildId = discordContext.GuildId;
            await _audioPlayer.JoinAsync(discordContext.VoiceChannel);
            MusicPlaylist musicPlaylist = InitializePlaylist(guildId);
            return await EnqueueTracks(guildId, musicPlaylist, playlistToAdd.PlaylistTracksInfo);
        }

        public async Task Stop(ulong guildId)
        {
            // This'll force the event to be sent, forcing a clean-up
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList)) guildPlayList.Clear();
            await _audioPlayer.SkipAsync(guildId);
        }

        public async Task Skip(ulong guildId)
        {
            await _audioPlayer.SkipAsync(guildId);
        }

        public async Task RemoveSong(ulong guildId, string trackId)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                guildPlayList.RemoveTrackById(trackId);
            }
        }

        public async Task GoToSong(ulong guildId, string trackId)
        {
            if (_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
            {
                guildPlayList.RemoveToTrackId(trackId);
                await _audioPlayer.SkipAsync(guildId);
            }
        }

        public bool TryGetPlaylist(ulong guildId, [NotNullWhen(true)] out IReadOnlyCollection<TrackInformations> playlist)
        {
            MusicPlaylist? guildPlayList = _currentPlaylists.GetValueOrDefault(guildId);

            if (guildPlayList == null)
            {
                playlist = new List<TrackInformations>();
                return false;
            }

            playlist = guildPlayList.Playlist;
            return true;
        }

        public Task<AudioPlayerState> GetAudioPlayerState(ulong guildId)
        {
            return _audioPlayer.GetPlayerState(guildId);
        }

        public async Task CleanUp(ulong guildId, IVoiceChannel? voiceChannel)
        {
            _currentPlaylists.Remove(guildId);
            await _audioPlayer.CleanUp(guildId, voiceChannel);
        }

        public async Task<TrackInformations?> PlayNext(ulong guildId)
        {
            if (!_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? guildPlayList))
                return null;
            
            while (!guildPlayList.IsEmpty)
            {
                TrackInformations? trackToPlay = guildPlayList.GetNextSongToPlay();

                if (trackToPlay != null)
                {
                    await _audioPlayer.PlayAsync(guildId, trackToPlay);
                    return trackToPlay;
                }
            }

            return null;
        }

        #endregion

        #region Helper
        private async Task<EPlayMusicResult> EnqueueTracks(ulong guildId, MusicPlaylist guildPlayList, List<TrackInformations> tracks)
        {
            guildPlayList.AddRange(tracks);
            AudioPlayerState res = await _audioPlayer.GetPlayerState(guildId);
            return res.IsPlaying ? EPlayMusicResult.Queued : EPlayMusicResult.NextToPlay;
        }

        private MusicPlaylist InitializePlaylist(ulong guildId)
        {   
            if (!_currentPlaylists.TryGetValue(guildId, out MusicPlaylist? musicPlaylist))
            {
                musicPlaylist = new MusicPlaylist();
                _currentPlaylists.Add(guildId, musicPlaylist);
            }

            return musicPlaylist;
        }
        #endregion
    }
}
