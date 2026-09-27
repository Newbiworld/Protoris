using Protoris.Data;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Protoris.Service.TrackResolver
{
    public class YoutubeTrackResolver : ITrackResolver
    {
        private readonly YoutubeClient _youtubeClient;
        public YoutubeTrackResolver()
        {
            _youtubeClient = new YoutubeClient();
        }

        public async Task<PlaylistInformations?> ResolvePlaylist(string url)
        {
            if (!url.Contains("youtube")) return null;


            // Only has youtube for now
            try
            {
                Playlist playlistMetadata = await _youtubeClient.Playlists.GetAsync(url);
                IReadOnlyCollection<PlaylistVideo> playlistVideosMetadata = await _youtubeClient.Playlists.GetVideosAsync(url).ToListAsync();
                PlaylistInformations playlistToAddInfo = new PlaylistInformations()
                {
                    PlaylistName = playlistMetadata.Title,
                    PlaylistUrl = playlistMetadata.Url,
                };

                
                foreach (PlaylistVideo videoMetadata in playlistVideosMetadata)
                {
                    playlistToAddInfo.PlaylistTracksInfo.Add(GetInfoFromMetaData(videoMetadata));
                }

                return playlistToAddInfo;
            }
            catch (Exception)
            {
                // Do nothing because we're cool and hips like that
            }


            return null;
        }

        public async Task<TrackInformations?> ResolveTrack(string url)
        {
            if (!url.Contains("youtube")) return null;

            try
            {
                Video playlistMetadata = await _youtubeClient.Videos.GetAsync(url);
                return GetInfoFromMetaData(playlistMetadata);
            }
            catch (Exception)
            {
                // Do nothing because we're cool and hips like that
            }

            return null;
        }


        private TrackInformations GetInfoFromMetaData(IVideo videoMetadata)
        {
            return new TrackInformations()
            {
                Artwork = videoMetadata.Thumbnails.FirstOrDefault()?.Url,
                Duration = videoMetadata.Duration!.Value,
                Title = videoMetadata.Title,
                Url = videoMetadata.Url,
            };
        }
    }
}
