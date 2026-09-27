using Discord;
using Google.Protobuf;

namespace Protoris.Data
{
    public class DiscordContext
    {
        public ulong GuildId { get; set; }
        public IVoiceChannel VoiceChannel { get; set; }
        public IMessageChannel CurrentMessageChannel { get; set; }
        public IGuildUser BotUser { get; set; }
        public List<IUserMessage> MessageHistory = new List<IUserMessage>();
        public CurrentPlayingMusic CurrentPlayingMusic { get; set; }

        public async Task RotatePlayingMusic(TrackInformations trackInfo, IUserMessage message)
        {
            if (CurrentPlayingMusic != null) await DeleteMessage(CurrentPlayingMusic.OriginalMessage);
            CurrentPlayingMusic = new CurrentPlayingMusic(trackInfo, message);
        }

        public void StartMusicTimer()
        {
            if (CurrentPlayingMusic != null)  CurrentPlayingMusic.StartedOn = DateTime.Now;
        }

        public async Task CleanUp()
        {
            foreach (IUserMessage message in MessageHistory)
                await DeleteMessage(message);

            await DeleteMessage(CurrentPlayingMusic.OriginalMessage);
        }

        private async Task DeleteMessage(IUserMessage message)
        {
            try
            {
                await message.DeleteAsync();
            }
            catch (Exception)
            {
                // We don't know the state of the message at this point, so like, blergh
            }
        }
    }
}
