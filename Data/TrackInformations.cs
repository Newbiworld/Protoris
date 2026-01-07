using Discord;
using Victoria;

namespace Protoris.Data
{
    public class TrackInformations
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public LavaTrack Track { get; set; }
        public IGuildUser RequestedBy { get; set; }
    }
}
