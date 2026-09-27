using Discord;
using Protoris.Data;
using Protoris.Enum;
using System.Diagnostics.CodeAnalysis;

namespace Protoris.Service.Interfaces
{
    public interface IMusicPlaylistService
    {
        public Task<EPlayMusicResult> AddMusic(DiscordContext discordContext, TrackInformations infos);
        public Task<EPlayMusicResult> AddMultipleMusic(DiscordContext discordContext, PlaylistInformations playlistToAdd);
        public Task<TrackInformations?> PlayNext(ulong guildId);
        public Task Stop(ulong guildId);
        public Task Skip(ulong guildId);
        public Task RemoveSong(ulong guildId, string trackId);
        public Task GoToSong(ulong guildId, string trackId);
        public bool TryGetPlaylist(ulong guildId, [NotNullWhen(true)] out IReadOnlyCollection<TrackInformations> playlist);
        public Task<IsInVcResponse> IsAudioPlayerInVc(ulong guildId);
        public Task CleanUp(ulong guildId, IVoiceChannel? voiceChannel);
    }
}
