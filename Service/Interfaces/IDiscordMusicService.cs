using Discord;
using Protoris.Enum;

namespace Protoris.Service.Interfaces
{
    public interface IDiscordMusicService
    {
        public Task PlayMusic(IDiscordInteraction interaction, IMessageChannel currentMessageChannel, string url);
        public Task HandleComponentInteraction(IComponentInteraction interaction, string trackId, EMusicInteraction musicInteraction);
        public Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand musicCommand);
    }
}
