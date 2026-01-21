using Discord;
using Protoris.Data;

namespace Protoris.Service.Interfaces
{
    public interface IMusicService
    {
        public Task AddMusic(IInteractionContext context, IVoiceChannel currentChannel, IMessageChannel currentMessageChannel, IGuildUser requestedByUser, string url);
        public Task AddMusics(IInteractionContext context, IVoiceChannel currentChannel, IMessageChannel currentMessageChannel, IGuildUser requestedByUser, PlaylistToAddInfo playlistToAdd);
        public Task Stop(IVoiceChannel currentChannel, IGuildUser requestedBy, IDiscordInteraction? interaction = null);
        public Task Skip(IDiscordInteraction interaction, IGuildUser requestedBy);
        public Task ShowPlaylist(IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0);
        public Task<ComponentBuilderV2> RemoveSong(IGuildUser requestedBy, string trackId);
        public Task ShowGoTo(IDiscordInteraction interaction, IGuildUser requestedBy, int index = 0);
        public Task<ComponentBuilderV2> GoToSong(IGuildUser requestedBy, string trackId);
        public Task<ComponentBuilderV2> ShowPlaylist(IGuildUser requestedBy, int index = 0);
        public Task<ComponentBuilderV2> ShowGoTo(IGuildUser requestedBy, int index = 0);
        public Task UpdateCurrentlyPlayingSongs();
    }
}
