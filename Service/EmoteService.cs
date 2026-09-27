using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Protoris.Data;
using Protoris.Service.Interfaces;

namespace Protoris.Service
{
    public class EmoteService : IEmoteService
    {
        private readonly Dictionary<string, ulong> _idsByEmote = new Dictionary<string, ulong>();
        public EmoteService(IConfiguration config)
        {
            if (config != null)
            {
                string? hasEmotes = config.GetValue<string>("HasEmotes");

                if (hasEmotes?.ToLower() == "true")
                {
                    _idsByEmote.Add(nameof(EzelSad), ulong.Parse(config.GetValue<string>("SadEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelCool), ulong.Parse(config.GetValue<string>("CoolEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelThink), ulong.Parse(config.GetValue<string>("ThinkingEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelSurprised), ulong.Parse(config.GetValue<string>("SurprisedEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelDisgust), ulong.Parse(config.GetValue<string>("DisgustEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelNervous), ulong.Parse(config.GetValue<string>("NervousEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelThinkWithCloud), ulong.Parse(config.GetValue<string>("ThinkingHardEzelEmote")!));
                    _idsByEmote.Add(nameof(ArrowRight), ulong.Parse(config.GetValue<string>("RightArrowEmote")!));
                    _idsByEmote.Add(nameof(Stop), ulong.Parse(config.GetValue<string>("StopEmote")!));
                    _idsByEmote.Add(nameof(Bin), ulong.Parse(config.GetValue<string>("DeleteEmote")!));
                    _idsByEmote.Add(nameof(EzelSleep), ulong.Parse(config.GetValue<string>("SleepingEzelEmote")!));
                    _idsByEmote.Add(nameof(EzelHeart), ulong.Parse(config.GetValue<string>("HeartEzelEmote")!));
                }
            }
        }

        public EmoteWithFallBack EzelSad { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelCool { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelThink { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelSurprised { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelDisgust { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelNervous { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelThinkWithCloud { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack ArrowRight { get; private set; } = new EmoteWithFallBack("=>");
        public EmoteWithFallBack Stop { get; private set; } = new EmoteWithFallBack("Stop");
        public EmoteWithFallBack Bin { get; private set; } = new EmoteWithFallBack("Delete");
        public EmoteWithFallBack EzelSleep { get; private set; } = new EmoteWithFallBack("");
        public EmoteWithFallBack EzelHeart { get; private set; } = new EmoteWithFallBack("");

        public async Task LoadEmotesAsync(DiscordSocketClient discordClient)
        {
            foreach (KeyValuePair<string, ulong> emoteIdWithName in _idsByEmote.ToList())
            {
                await AssignEmoteWithReflection(discordClient, emoteIdWithName.Key, emoteIdWithName.Value);
            }
        }

        private async Task AssignEmoteWithReflection(DiscordSocketClient discordClient, string emoteName, ulong emoteId)
        {
            Emote emote = await discordClient.GetApplicationEmoteAsync(emoteId);
            object? fallBackEmote = GetType().GetProperty(emoteName)!.GetValue(this);
            fallBackEmote!.GetType().GetProperty(nameof(EmoteWithFallBack.Emote))!.SetValue(fallBackEmote, emote);
        }
    }
}
