using Protoris.Data;
using Protoris.Enum;
using Protoris.Service.Config;
using SpotifyAPI.Web;
using YoutubeExplode.Playlists;

namespace Protoris.Service.TrackResolver
{
    public class SpotifyTrackResolver : ITrackResolver
    {
        private readonly SpotifyClient _spotifyClient;

        public SpotifyTrackResolver(ISpotifyConfig spotifyClientConfig)
        {
            SpotifyClientConfig config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new ClientCredentialsAuthenticator(spotifyClientConfig.SpotifyClientId, spotifyClientConfig.SpotifyClientSecret));

            _spotifyClient = new SpotifyClient(config);
        }

        public async Task<PlaylistInformations?> ResolvePlaylist(string url)
        {
            try
            {
                ESpotifyRequestType requestType = GetSpotifyRequestTypeFromUrl(url, out string spotifyId);
                if (requestType == ESpotifyRequestType.Album) return await HandleAlbum(spotifyId, url);
                if (requestType == ESpotifyRequestType.Playlist) return await HandlePlaylist(spotifyId, url);
            }
            catch (Exception) { }
            return null;
        }

        public async Task<TrackInformations?> ResolveTrack(string url)
        {
            ESpotifyRequestType requestType = GetSpotifyRequestTypeFromUrl(url, out string spotifyId);
            if (requestType != ESpotifyRequestType.Track) return null;

            try
            {
                FullTrack track = await _spotifyClient.Tracks.Get(spotifyId);
                return ConvertToTrackInformations(track);
            }
            catch (Exception) { }
            return null;
        }

        private async Task<PlaylistInformations?> HandleAlbum(string spotifyId, string url)
        {
            FullAlbum album = await _spotifyClient.Albums.Get(spotifyId);

            if (album?.Tracks?.Total > 0)
            {
                PlaylistInformations playlistToAddInfo = new PlaylistInformations()
                {
                    PlaylistName = album.Name,
                    PlaylistUrl = album.ExternalUrls?["spotify"],
                };

                Paging<SimpleTrack>? paging = album.Tracks;
                string? albumCover = album.Images.FirstOrDefault()?.Url;

                while (paging != null)
                {
                    if (paging?.Items == null) return playlistToAddInfo;

                    foreach (SimpleTrack track in paging.Items)
                    {
                        playlistToAddInfo.PlaylistTracksInfo.Add(ConvertToTrackInformations(track, albumCover));
                    }

                    if (paging.Next != null) paging = await _spotifyClient.NextPage(paging);
                    else paging = null;
                }

                return playlistToAddInfo;
            }

            return null;
        }

        // Will not works: need to be a user to read it
        private async Task<PlaylistInformations?> HandlePlaylist(string spotifyId, string url)
        {
            FullPlaylist playlist = await _spotifyClient.Playlists.Get(spotifyId);

            if (playlist?.Items?.Total > 0)
            {
                PlaylistInformations playlistToAddInfo = new PlaylistInformations()
                {
                    PlaylistName = playlist.Name,
                    PlaylistUrl = playlist.ExternalUrls?["spotify"],
                };

                Paging<PlaylistTrack<IPlayableItem>>? paging = playlist.Items;
                string? albumCover = playlist.Images?.FirstOrDefault()?.Url;

                while (paging != null)
                {
                    if (paging?.Items == null) return playlistToAddInfo;

                    foreach (PlaylistTrack<IPlayableItem> track in paging.Items)
                    {
                        ItemType trackType = track.Track.Type;
                        if (trackType == ItemType.Track)
                            playlistToAddInfo.PlaylistTracksInfo.Add(ConvertToTrackInformations((track.Track as FullTrack)!));
                    }

                    if (paging.Next != null) paging = await _spotifyClient.NextPage(paging);
                    else paging = null;
                }

                return playlistToAddInfo;
            }

            return null;
        }

        private TrackInformations ConvertToTrackInformations(FullTrack track)
        {
            return new TrackInformations()
            {
                Artwork = track.Album?.Images?.FirstOrDefault()?.Url,
                Title = track.Name,
                Duration = TimeSpan.FromMilliseconds(track.DurationMs),
                Url = track.ExternalUrls["spotify"],
            };
        }

        private TrackInformations ConvertToTrackInformations(SimpleTrack track, string? iconUrl)
        {
            return new TrackInformations()
            {
                Artwork = iconUrl,
                Title = track.Name,
                Duration = TimeSpan.FromMilliseconds(track.DurationMs),
                Url = track.ExternalUrls["spotify"],
            };
        }

        private ESpotifyRequestType GetSpotifyRequestTypeFromUrl(string url, out string spotifyId)
        {
            const string spotifyStartUrl = "https://open.spotify.com/";
            spotifyId = string.Empty;

            if (!url.StartsWith(spotifyStartUrl)) return ESpotifyRequestType.Unknown;
            
            string fixedParts = url.Replace(spotifyStartUrl, string.Empty);
            string[] separated = fixedParts.Split("/");

            if (separated.Length != 2) return ESpotifyRequestType.Unknown;

            string type = separated[0];

            spotifyId = separated[1];
            int index = spotifyId.IndexOf("?");
            if (index >= 0) spotifyId = spotifyId.Substring(0, index);

            return separated[0] switch
            {
                "track" => ESpotifyRequestType.Track,
                "album" => ESpotifyRequestType.Album,
                "playlist" => ESpotifyRequestType.Playlist,
                _ => ESpotifyRequestType.Unknown,
            };
        }
    }
}
