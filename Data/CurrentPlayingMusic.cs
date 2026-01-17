using Discord;

namespace Protoris.Data
{
    public class CurrentPlayingMusic
    {
        public ulong GuildId { get; set; }
        public TrackInformations TrackInfo { get; set; }
        public IUserMessage OriginalMessage { get; set; }
        public DateTime StartedOn { get; set; }

        public CurrentPlayingMusic(TrackInformations trackInfo, IUserMessage originalMessage, ulong guildId)
        {
            TrackInfo = trackInfo;
            OriginalMessage = originalMessage;
            StartedOn = DateTime.Now;
            GuildId = guildId;
        }

        public TimeSpan GetCurrentTime()
        {
            return DateTime.Now - StartedOn;
        }
    }
}
