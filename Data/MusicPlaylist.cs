namespace Protoris.Data
{
    public class MusicPlaylist
    {
        private List<TrackInformations> _playlist { get; set; } = new List<TrackInformations>();
        public IReadOnlyCollection<TrackInformations> Playlist => _playlist;

        public void RemoveTrackById(string id)
        {
            TrackInformations? trackInfo = _playlist.FirstOrDefault(x => x.Id == id);
            if (trackInfo != null) _playlist.Remove(trackInfo);
        }

        public void RemoveToTrackId(string id)
        {
            TrackInformations? trackToGoTo = _playlist.FirstOrDefault(x => x.Id == id);

            if (trackToGoTo != null)
            {
                _playlist.RemoveRange(0, _playlist.IndexOf(trackToGoTo));
            }
        }

        public void AddRange(List<TrackInformations> infos)
        {
            _playlist.AddRange(infos);
        }

        public void Add(TrackInformations info)
        {
            _playlist.Add(info);
        }

        public void Clear()
        {
            _playlist.Clear();
        }

        public bool IsEmpty => _playlist.Count == 0;
        public TrackInformations? GetNextSongToPlay()
        {
            if (IsEmpty) return null;
            TrackInformations topTrack = _playlist[0];
            _playlist.RemoveAt(0);
            return topTrack;
        }
    }
}
