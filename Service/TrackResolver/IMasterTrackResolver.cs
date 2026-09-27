using Discord;
using Protoris.Data;

namespace Protoris.Service.TrackResolver
{
    public interface IMasterTrackResolver
    {
        public Task<TrackInformations?> ResolveTrack(string url, IGuildUser requestedByUser);
        public Task<PlaylistInformations?> ResolvePlaylist(string url, IGuildUser requestedByUser);
    }
}
