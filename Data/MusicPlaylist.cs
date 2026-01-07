using Discord;
using Victoria;

namespace Protoris.Data
{
    public class MusicPlaylist
    {
        public ulong GuildId { get; set; }
        public IMessageChannel OriginationMessageChannel { get; set; } // Place to send updates
        public IVoiceChannel CurrentVoiceChannel { get; set; } // Current vc (used for disconnection)
        public List<TrackInformations> Playlist { get; set; } = new List<TrackInformations>();
    }
}
