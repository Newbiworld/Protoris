using Protoris.Data;

namespace Protoris.Service.TrackResolver
{
    public interface ITrackResolver
    {
        public Task<TrackInformations?> ResolveTrack(string url);
        public Task<PlaylistInformations?> ResolvePlaylist(string url);
    }
}
