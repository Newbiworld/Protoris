using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoris.Service.Interfaces
{
    public class FakeEmoteService : IEmoteService
    {
        public Emote EzelHeart => throw new NotImplementedException();

        public Emote EzelSleep => throw new NotImplementedException();

        public Emote EzelThinkWithCloud => throw new NotImplementedException();

        public Emote EzelNervous => throw new NotImplementedException();

        public Emote EzelDisgust => throw new NotImplementedException();

        public Emote EzelThink => throw new NotImplementedException();

        public Emote EzelSurprised => throw new NotImplementedException();

        public Emote EzelSad => throw new NotImplementedException();

        public Emote EzelCool => throw new NotImplementedException();

        public Emote ArrowRight => throw new NotImplementedException();

        public Emote Stop => throw new NotImplementedException();

        public Emote Bin => throw new NotImplementedException();

        public Task LoadEmotesAsync(DiscordSocketClient discordClient)
        {
            // Empty so the injection of dependancy works
            return Task.CompletedTask;
        }
    }
}
