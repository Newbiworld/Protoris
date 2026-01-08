using Discord;

namespace Protoris.Service.Interfaces
{
    public interface IMusicService
    {
        public Task AddMusic(IDiscordInteraction interaction, IVoiceChannel currentChannel, IMessageChannel currentMessageChannel, IGuildUser requestedByUser, string url);
        public Task<ComponentBuilderV2> Stop(ulong guildId, IVoiceChannel currentChannel, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> Skip(ulong guildId, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> ShowPlaylist(ulong guildId, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> RemoveSong(ulong guildId, IGuildUser requestedBy, string trackId);
        public Task<ComponentBuilderV2> ShowGoTo(ulong guildId, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> GoToSong(ulong guildId, IGuildUser requestedBy, string trackId);
        public Task UpdateCurrentlyPlayingSongs();
    }
}
