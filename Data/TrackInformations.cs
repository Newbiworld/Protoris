using Discord;
using Victoria;

namespace Protoris.Data
{
    public class TrackInformations
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string Url { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Artwork { get; set; }
        public IGuildUser RequestedBy { get; set; }
    }
}
