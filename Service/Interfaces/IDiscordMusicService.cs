using Discord;
using Protoris.Enum;

namespace Protoris.Service.Interfaces
{
    public interface IDiscordMusicService
    {
        public Task PlayMusic(IInteractionContext context, IMessageChannel currentMessageChannel, string url);
        public Task MultiplePlayMusic(IInteractionContext context, IMessageChannel currentMessageChannel, string url);
        public Task HandleComponentInteraction(IComponentInteraction interaction, string param, EMusicInteraction musicInteraction);
        public Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand musicCommand, int index = 0);
    }
}
