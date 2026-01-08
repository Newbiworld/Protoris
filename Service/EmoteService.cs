using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
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

        public Emote EzelSad { get; private set; }
        public Emote EzelCool { get; private set; }
        public Emote EzelThink { get; private set; }
        public Emote EzelSurprised { get; private set; }
        public Emote EzelDisgust { get; private set; }
        public Emote EzelNervous { get; private set; }
        public Emote EzelThinkWithCloud { get; private set; }
        public Emote ArrowRight { get; private set; }
        public Emote Stop { get; private set; }
        public Emote Bin { get; private set; }
        public Emote EzelSleep { get; private set; }
        public Emote EzelHeart { get; private set; }

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
            GetType().GetProperty(emoteName)!.SetValue(this, emote);
        }
    }
}
