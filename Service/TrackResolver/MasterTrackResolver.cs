using Discord;
using Protoris.Data;
using Protoris.Extensions;

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
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) return null;

            foreach (ITrackResolver trackResolver in _trackResolvers)
            {
                PlaylistInformations? playlistInformation = await trackResolver.ResolvePlaylist(uri);
                if (playlistInformation != null)
                {
                    playlistInformation.RequestedBy = requestedByUser;
                    playlistInformation.PlaylistTracksInfo.ForEach(track => track.RequestedBy = requestedByUser.GetNicknameOrUsername());
                    return playlistInformation;
                }
            }

            return null;
        }

        public async Task<TrackInformations?> ResolveTrack(string url, IGuildUser requestedByUser)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) return null;

            foreach (ITrackResolver trackResolver in _trackResolvers)
            {
                TrackInformations? trackInformations = await trackResolver.ResolveTrack(uri);
                if (trackInformations != null)
                {
                    trackInformations.RequestedBy = requestedByUser.GetNicknameOrUsername();
                    return trackInformations;
                }
            }

            return null;
        }
    }
}
