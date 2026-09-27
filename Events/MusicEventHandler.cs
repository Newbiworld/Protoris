using Microsoft.Extensions.Hosting;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Events.Data;
using Protoris.Service.Interfaces;

namespace Protoris.Events
{
    public class MusicEventHandler : IHostedService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly IDiscordMusicService _discordMusicService;
        private readonly IMusicPlaylistService _playlistService;

        public MusicEventHandler(IAudioPlayer audioPlayer, IDiscordMusicService discordMusicService, IMusicPlaylistService playlistService)
        {
            _audioPlayer = audioPlayer;
            _playlistService = playlistService;
            _discordMusicService = discordMusicService;
        }

        public void OnMusicChange(object? sender, MusicStatusChangedArgs e)
        {
            switch (e.MusicStatus)
            {
                case EMusicStatus.OnSongStuck:
                case EMusicStatus.OnMusicEnd:
                case EMusicStatus.OnTrackException:
                    HandleMusicChange(e.GuildId);
                    break;
                case EMusicStatus.OnTrackStart:
                    _discordMusicService.StartTimerForCurrentSong(e.GuildId);
                    break;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _audioPlayer.MusicStatusChanged += OnMusicChange;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _audioPlayer.MusicStatusChanged -= OnMusicChange;
        }

        private async Task HandleMusicChange(ulong guildId)
        {
            AudioPlayerState isInVcResponse = await _audioPlayer.GetPlayerState(guildId);
            _playlistService.TryGetPlaylist(guildId, out IReadOnlyCollection<TrackInformations> playlistInformations);

            if (!isInVcResponse.IsInVc || (!isInVcResponse.IsPlaying && playlistInformations.Count == 0))
            {
                await _discordMusicService.CleanUpAndFarewell(guildId);
            }
            else
            {
                await _discordMusicService.PlayNextSong(guildId);
            }
        }
    }
}
