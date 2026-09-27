using Discord;

namespace Protoris.Data
{
    public class PlaylistInformations
    {
        public string PlaylistName { get; set; }
        public string PlaylistUrl { get; set; }
        public List<TrackInformations> PlaylistTracksInfo { get; set; } = new List<TrackInformations>();
        public IGuildUser RequestedBy { get; set; }
    }
}
