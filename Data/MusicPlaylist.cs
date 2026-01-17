using Discord;

namespace Protoris.Data
{
    public class MusicPlaylist
    {
        public ulong GuildId { get; set; }
        public IGuildUser BotUser { get; set; }
        public IMessageChannel OriginationMessageChannel { get; set; } // Place to send updates
        public IVoiceChannel CurrentVoiceChannel { get; set; } // Current vc (used for disconnection)
        public List<TrackInformations> Playlist { get; set; } = new List<TrackInformations>();
        public List<IUserMessage> SentMessages { get; set; } = new List<IUserMessage>();
    }
}
