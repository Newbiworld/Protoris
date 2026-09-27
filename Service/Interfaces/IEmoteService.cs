using Discord;
using Discord.WebSocket;
using Protoris.Data;

namespace Protoris.Service.Interfaces
{
    public interface IEmoteService
    {
        public Task LoadEmotesAsync(DiscordSocketClient discordClient);
        public EmoteWithFallBack EzelHeart { get; }
        public EmoteWithFallBack EzelSleep { get; }
        public EmoteWithFallBack EzelThinkWithCloud { get; }
        public EmoteWithFallBack EzelNervous { get; }
        public EmoteWithFallBack EzelDisgust { get; }
        public EmoteWithFallBack EzelThink { get; }
        public EmoteWithFallBack EzelSurprised { get; }
        public EmoteWithFallBack EzelSad { get; }
        public EmoteWithFallBack EzelCool { get; }
        public EmoteWithFallBack ArrowRight { get; }
        public EmoteWithFallBack Stop { get; }
        public EmoteWithFallBack Bin { get; }
    }
}
