using Discord;

namespace Protoris.Service.Interfaces
{
    public interface IDiscordMusicService
    {
        public Task PlayMusic(IDiscordInteraction interaction, IMessageChannel currentMessageChannel, string url);
        public Task StopMusic(IDiscordInteraction interaction);
        public Task SkipMusic(IDiscordInteraction interaction);
        public Task ShowPlaylist(IDiscordInteraction interaction);
        public Task RemoveSong(IComponentInteraction interaction, string trackId);
        public Task ShowGoTo(IDiscordInteraction interaction);
        public Task GoToTrack(IComponentInteraction interaction, string trackId);
    }
}
