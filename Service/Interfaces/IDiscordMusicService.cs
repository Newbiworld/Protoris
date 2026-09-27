using Discord;
using Protoris.Enum;

namespace Protoris.Service.Interfaces
{
    public interface IDiscordMusicService
    {
        public Task PlayMusic(IInteractionContext context, string url);
        public Task MultiplePlayMusic(IInteractionContext context, string url);
        public Task HandleComponentInteraction(IComponentInteraction interaction, string param, EMusicInteraction musicInteraction);
        public Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand musicCommand);
        public Task CleanUpAndFarewell(ulong guildId);
        public Task UpdateCurrentlyPlayingSongs();
        public Task PlayNextSong(ulong guildId, IDiscordInteraction? interaction = null);
        public void StartTimerForCurrentSong(ulong guildId);
    }
}
