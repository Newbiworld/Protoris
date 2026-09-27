using Discord;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Events.Data;
using Protoris.Service.Interfaces;
using Victoria;
using Victoria.WebSocket.EventArgs;

namespace Protoris.Service
{
    public class LavaLinkAudioPlayer : IAudioPlayer
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly ITrackPreloader _trackPreloader;

        public LavaLinkAudioPlayer(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava, ITrackPreloader trackPreloader)
        {
            _lava = lava;
            _lava.OnTrackEnd += OnEndOfSong;
            _lava.OnTrackStuck += OnSongStuck;
            _lava.OnTrackException += OnTrackException;
            _lava.OnTrackStart += OnTrackStart;
            _trackPreloader = trackPreloader;
        }

        public async Task JoinAsync(IVoiceChannel voiceChannel)
        {
            ulong guildId = voiceChannel.GuildId;
            LavaPlayer<LavaTrack>? lavaPlayer = await _lava.TryGetPlayerAsync(guildId);

            // If null, the player don't exist, so the bot is not in a vc in the guild
            if (lavaPlayer == null || !lavaPlayer.State.IsConnected)
            {
                lavaPlayer = await _lava.JoinAsync(voiceChannel);
            }
        }

        public async Task PlayAsync(ulong guildId, TrackInformations trackInfo)
        {
            await _trackPreloader.PlayTrack(guildId, trackInfo);
        }

        public async Task SkipAsync(ulong guildId)
        {
            LavaPlayer<LavaTrack>? player = await _lava.TryGetPlayerAsync(guildId);
            if (player?.Track?.Duration != null)
            {
                TimeSpan duration = player.Track.Duration;
                if (duration.Seconds < 0) OnMusicStatusChanged(EMusicStatus.OnMusicEnd, guildId); // We can't skip it manually, so we just force it
                else await player.SeekAsync(_lava, player.Track.Duration);

            }
        }

        public async Task StopAsync(ulong guildId, IVoiceChannel voiceChannel)
        {
            await SkipAsync(guildId);
            await _lava.LeaveAsync(voiceChannel);
        }

        public async Task<AudioPlayerState> GetPlayerState(ulong guildId)
        {
            LavaPlayer<LavaTrack>? lavaPlayer = await _lava.TryGetPlayerAsync(guildId);
            if (lavaPlayer == null)
            {
                return new AudioPlayerState()
                {
                    IsInVc = false,
                    IsPlaying = false,
                    Reason = "Wow, I don't even exist!"
                };
            }

            if (!lavaPlayer.State.IsConnected)
            {
                return new AudioPlayerState()
                {
                    IsInVc = true,
                    IsPlaying = false,
                    Reason = "Wow, don't tell me to do stuff when I ain't even on stage!"
                };
            }

            if (lavaPlayer.Track == null)
            {
                return new AudioPlayerState()
                {
                    IsInVc = true,
                    IsPlaying = false,
                    Reason = "Wow, don't tell me what to do when I ain't singing! ... rude"
                };
            }

            return new AudioPlayerState()
            {
                IsInVc =  true,
                IsPlaying = true
            };
        }

        public async Task CleanUp(ulong guildId, IVoiceChannel? voiceChannel)
        {
            await SkipAsync(guildId);
            if (voiceChannel != null) await _lava.LeaveAsync(voiceChannel);
            _trackPreloader.CleanUp(guildId);
        }

        #region Events
        protected void OnMusicStatusChanged(EMusicStatus status, ulong guildId)
        {
            MusicStatusChangedArgs musicStatusChanged = new MusicStatusChangedArgs()
            {
                GuildId = guildId,
                MusicStatus = status
            };

            MusicStatusChanged?.Invoke(this, musicStatusChanged);
        }

        public event EventHandler<MusicStatusChangedArgs>? MusicStatusChanged;

        private async Task OnEndOfSong(TrackEndEventArg eventArgument)
        {
            OnMusicStatusChanged(EMusicStatus.OnMusicEnd, eventArgument.GuildId);
        }

        private async Task OnSongStuck(TrackStuckEventArg eventArgument)
        {
            OnMusicStatusChanged(EMusicStatus.OnSongStuck, eventArgument.GuildId);
        }

        private async Task OnTrackException(TrackExceptionEventArg eventArgument)
        {
            OnMusicStatusChanged(EMusicStatus.OnTrackException, eventArgument.GuildId);
        }

        private async Task OnTrackStart(TrackStartEventArg eventArgument)
        {
            OnMusicStatusChanged(EMusicStatus.OnTrackStart, eventArgument.GuildId);
        }
        #endregion
    }
}
