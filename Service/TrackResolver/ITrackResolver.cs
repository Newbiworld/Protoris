using Protoris.Data;

namespace Protoris.Service.TrackResolver
{
    public interface ITrackResolver
    {
        public Task<TrackInformations?> ResolveTrack(Uri uri);
        public Task<PlaylistInformations?> ResolvePlaylist(Uri uri);
    }
}
