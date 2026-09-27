using Protoris.Data;
using Protoris.Service.Interfaces;
using Victoria;
using Victoria.Rest.Search;

namespace Protoris.Service
{
    public class LavaLinkTrackPreLoader : ITrackPreloader
    {
        public Dictionary<ulong, Dictionary<string, LavaTrack>> _preLoadedTracks = new Dictionary<ulong, Dictionary<string, LavaTrack>>();
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;

        public LavaLinkTrackPreLoader(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava)
        {
            _lava = lava;
        }

        public void CleanUp(ulong guildId)
        {
            _preLoadedTracks.Remove(guildId);
        }

        public async Task<PlaylistInformations?> PreloadMultipleTracks(ulong guildId, PlaylistInformations? playlistInformations)
        {
            if (playlistInformations == null) return null;

            List<TrackInformations> foundTrack = new List<TrackInformations>();
            Dictionary<string, LavaTrack> preLoadedTracksForGuild = GetOrStartContext(guildId);

            foreach (TrackInformations track in playlistInformations.PlaylistTracksInfo)
            {
                LavaTrack? lavaTrack = await GetTrack(track);

                if (lavaTrack != null)
                {
                    preLoadedTracksForGuild.Add(track.Id, lavaTrack);
                    foundTrack.Add(track);
                }
            }

            playlistInformations.PlaylistTracksInfo = foundTrack;
            return playlistInformations;
        }

        public async Task<TrackInformations?> PreloadTrack(ulong guildId, TrackInformations? track)
        {
            if (track == null) return null;
            LavaTrack? lavaTrack = await GetTrack(track);

            if (lavaTrack != null)
            {
                Dictionary<string, LavaTrack> preLoadedTracksForGuild = GetOrStartContext(guildId);
                preLoadedTracksForGuild.Add(track.Id, lavaTrack);
                return track;
            }

            return null;
        }

        public async Task PlayTrack(ulong guildId, TrackInformations trackInfo)
        {
            if (_preLoadedTracks.TryGetValue(guildId, out Dictionary<string, LavaTrack>? tracksById))
            {
                if (tracksById.TryGetValue(trackInfo.Id, out LavaTrack? trackToPlay))
                {
                    LavaPlayer<LavaTrack> lavaPlayer = await _lava.TryGetPlayerAsync(guildId);
                    if (lavaPlayer != null) await lavaPlayer.PlayAsync(_lava, trackToPlay);
                }
            }
        }

        private async Task<LavaTrack?> GetTrack(TrackInformations track)
        {
            SearchResponse searchResponse = await _lava.LoadTrackAsync(track.Url);

            if (searchResponse.Type == SearchType.Empty || searchResponse.Type == SearchType.Error)
                return null;

            if (searchResponse.Tracks.Any())
            {
                return searchResponse.Tracks.First();
            }

            return null;
        }

        private Dictionary<string, LavaTrack> GetOrStartContext(ulong guildId)
        {
            if (!_preLoadedTracks.TryGetValue(guildId, out Dictionary<string, LavaTrack>? preloadedTracks))
            {
                preloadedTracks = new Dictionary<string, LavaTrack>();
                _preLoadedTracks.Add(guildId, preloadedTracks);
            }

            return preloadedTracks;
        }
    }
}
