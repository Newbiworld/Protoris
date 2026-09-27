using Protoris.Data;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Protoris.Service.TrackResolver
{
    public class YoutubeTrackResolver : ITrackResolver
    {
        private readonly YoutubeClient _youtubeClient;
        private readonly List<string> _validYoutubeHostName = ["youtube.com", "m.youtube.com", "youtu.be", "www.youtube.com"];
        public YoutubeTrackResolver()
        {
            _youtubeClient = new YoutubeClient();
        }

        public async Task<PlaylistInformations?> ResolvePlaylist(Uri uri)
        {
            if (!_validYoutubeHostName.Contains(uri.Host)) return null;

            try
            {
                Playlist playlistMetadata = await _youtubeClient.Playlists.GetAsync(uri.OriginalString);
                IReadOnlyCollection<PlaylistVideo> playlistVideosMetadata = await _youtubeClient.Playlists.GetVideosAsync(uri.OriginalString).ToListAsync();
                PlaylistInformations playlistToAddInfo = new PlaylistInformations()
                {
                    PlaylistName = playlistMetadata.Title,
                    PlaylistUrl = uri.OriginalString,
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

        public async Task<TrackInformations?> ResolveTrack(Uri uri)
        {
            if (!_validYoutubeHostName.Contains(uri.Host)) return null;

            try
            {
                Video playlistMetadata = await _youtubeClient.Videos.GetAsync(uri.OriginalString);
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
