using Protoris.Data;
using Victoria;
using Victoria.Rest.Search;

namespace Protoris.Service.TrackResolver
{
    // Last resolver if all other fails: take longers than the others.
    public class LavalinkTrackResolver : ITrackResolver
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;

        public LavalinkTrackResolver(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava)
        {
            _lava = lava;
        }

        public async Task<PlaylistInformations?> ResolvePlaylist(string url)
        {
            SearchResponse searchResponse = await _lava.LoadTrackAsync(url);

            if (searchResponse.Type == SearchType.Error || searchResponse.Type == SearchType.Empty)
                return null;

            List<TrackInformations> tracks = searchResponse.Tracks.Select(x => ExtractInfoFromTrack(x)).ToList();
            
            return new PlaylistInformations()
            {
                PlaylistName = searchResponse.Playlist.Name,
                PlaylistTracksInfo = tracks,
                PlaylistUrl = url,
            };
        }

        public async Task<TrackInformations?> ResolveTrack(string url)
        {
            SearchResponse searchResponse = await _lava.LoadTrackAsync(url);

            if (searchResponse.Type == SearchType.Error || searchResponse.Type == SearchType.Empty)
                return null;

            return ExtractInfoFromTrack(searchResponse.Tracks.First());
        }

        private TrackInformations ExtractInfoFromTrack(LavaTrack lavaTrack)
        {
            return new TrackInformations()
            {
                Duration = lavaTrack.Duration,
                Artwork = lavaTrack.Artwork,
                Title = lavaTrack.Title,
                Url = lavaTrack.Url,
            };
        }
    }
}
