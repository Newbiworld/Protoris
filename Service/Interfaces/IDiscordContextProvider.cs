using Discord;
using Protoris.Data;
using System.Diagnostics.CodeAnalysis;

namespace Protoris.Service.Interfaces
{
    public interface IDiscordContextProvider
    {
        public Task<DiscordContext> CreateDiscordContext(IInteractionContext interactionContext, IVoiceChannel userVoiceChannel);
        public bool TryGetContext(ulong guildId, [NotNullWhen(true)] out DiscordContext context);
        public void Remove(ulong guildId);
        public List<DiscordContext> GetAllContext();
    }
}
