using Protoris.Data;

namespace Protoris.Service.Interfaces
{
    public interface ITrackPreloader
    {
        public Task<PlaylistInformations?> PreloadMultipleTracks(ulong guildId, PlaylistInformations? playlistInformations);
        public Task<TrackInformations?> PreloadTrack(ulong guildId, TrackInformations? track);
        public Task PlayTrack(ulong guildId, TrackInformations trackInfo);
        public void CleanUp(ulong guildId);
    }
}
