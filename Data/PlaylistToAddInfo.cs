using Discord;
using Victoria;

namespace Protoris.Data
{
    public class PlaylistToAddInfo
    {
        public string PlaylistName { get; set; }
        public string PlaylistUrl { get; set; }
        public List<string> PlaylistVideosUrl { get; set; } = new List<string>();
        public List<TrackInformations> PlaylistTracksInfo { get; set; } = new List<TrackInformations>();
        public IGuildUser RequestedBy { get; set; }
    }
}
