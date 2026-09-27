using Discord;
using Protoris.Data;

namespace Protoris.Service.TrackResolver
{
    public class MasterTrackResolver : IMasterTrackResolver
    {
        private readonly IEnumerable<ITrackResolver> _trackResolvers;
        public MasterTrackResolver(IEnumerable<ITrackResolver> trackResolvers)
        {
            _trackResolvers = trackResolvers;
        }

        public async Task<PlaylistInformations?> ResolvePlaylist(string url, IGuildUser requestedByUser)
        {
            foreach (ITrackResolver trackResolver in _trackResolvers)
            {
                PlaylistInformations? playlistInformation = await trackResolver.ResolvePlaylist(url);
                if (playlistInformation != null)
                {
                    playlistInformation.RequestedBy = requestedByUser;
                    playlistInformation.PlaylistTracksInfo.ForEach(track => track.RequestedBy = requestedByUser);
                    return playlistInformation;
                }
            }

            return null;
        }

        public async Task<TrackInformations?> ResolveTrack(string url, IGuildUser requestedByUser)
        {
            foreach (ITrackResolver trackResolver in _trackResolvers)
            {
                TrackInformations? trackInformations = await trackResolver.ResolveTrack(url);
                if (trackInformations != null)
                {
                    trackInformations.RequestedBy = requestedByUser;
                    return trackInformations;
                }
            }

            return null;
        }
    }
}
