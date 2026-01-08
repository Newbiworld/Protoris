using Discord;

namespace Protoris.Service.Interfaces
{
    public interface IMusicService
    {
        public Task AddMusic(IDiscordInteraction interaction, IVoiceChannel currentChannel, IMessageChannel currentMessageChannel, IGuildUser requestedByUser, string url);
        public Task Stop(IVoiceChannel currentChannel, IGuildUser requestedBy, IDiscordInteraction? interaction = null);
        public Task Skip(IDiscordInteraction interaction, IGuildUser requestedBy);
        public Task ShowPlaylist(IDiscordInteraction interaction, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> RemoveSong(IGuildUser requestedBy, string trackId);
        public Task ShowGoTo(IDiscordInteraction interaction, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> GoToSong(IGuildUser requestedBy, string trackId);
        public Task UpdateCurrentlyPlayingSongs();
    }
}
