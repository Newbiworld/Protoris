using Discord;

namespace Protoris.Data
{
    public class CurrentPlayingMusic
    {
        public TrackInformations TrackInfo { get; set; }
        public IUserMessage OriginalMessage { get; set; }
        public DateTime StartedOn { get; set; }

        public CurrentPlayingMusic(TrackInformations trackInfo, IUserMessage originalMessage)
        {
            TrackInfo = trackInfo;
            OriginalMessage = originalMessage;
            StartedOn = DateTime.Now;
        }

        public TimeSpan GetCurrentTime()
        {
            return DateTime.Now - StartedOn;
        }
    }
}
