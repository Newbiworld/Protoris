using Discord;
using Protoris.Data;
using Protoris.Service.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Protoris.Service
{
    public class DiscordContextProvider : IDiscordContextProvider
    {
        private Dictionary<ulong, DiscordContext> _discordContextByGuildId = new Dictionary<ulong, DiscordContext>();

        public async Task<DiscordContext> CreateDiscordContext(IInteractionContext interactionContext, IVoiceChannel userVoiceChannel)
        {
            IGuildUser botUser = await interactionContext.Guild.GetCurrentUserAsync();

            DiscordContext context = new DiscordContext()
            {
                GuildId = interactionContext.Guild.Id,
                CurrentMessageChannel = interactionContext.Channel,
                VoiceChannel = userVoiceChannel,
                BotUser = botUser,
            };

            _discordContextByGuildId.Add(interactionContext.Guild.Id, context);

            return context;
        }

        public List<DiscordContext> GetAllContext()
        {
            return _discordContextByGuildId.Values.ToList();
        }

        public bool TryGetContext(ulong guildId, [NotNullWhen(true)] out DiscordContext context)
        {
            return _discordContextByGuildId.TryGetValue(guildId, out context);
        }

        public void Remove(ulong guildId)
        {
            _discordContextByGuildId.Remove(guildId);
        }
    }
}
