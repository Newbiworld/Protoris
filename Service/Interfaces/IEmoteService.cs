using Discord;
using Discord.WebSocket;

namespace Protoris.Service.Interfaces
{
    public interface IEmoteService
    {
        public Task LoadEmotesAsync(DiscordSocketClient discordClient);
        public Emote EzelHeart { get; }
        public Emote EzelSleep { get; }
        public Emote EzelThinkWithCloud { get; }
        public Emote EzelNervous { get; }
        public Emote EzelDisgust { get; }
        public Emote EzelThink { get; }
        public Emote EzelSurprised { get; }
        public Emote EzelSad { get; }
        public Emote EzelCool { get; }
        public Emote ArrowRight { get; }
        public Emote Stop { get; }
        public Emote Bin { get; }
    }
}
