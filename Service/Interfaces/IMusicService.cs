using Discord;
using Victoria;

namespace Protoris.Service.Interfaces
{
    public interface IMusicService
    {
        public Task<ComponentBuilderV2> AddMusic(IVoiceChannel currentChannel, IMessageChannel currentMessageChannel, IGuildUser requestedByUser, string url);
        public Task Stop(ulong guildId, IVoiceChannel currentChannel);
        public Task Skip(ulong guildId);
        public Task<ComponentBuilderV2> ShowPlaylist(ulong guildId, IGuildUser requestedBy);
        public void RemoveSong(ulong guildId, IGuildUser requestedBy, string trackId);
        public Task<ComponentBuilderV2> ShowGoTo(ulong guildId, IGuildUser requestedBy);
        public Task GoToSong(ulong guildId, IGuildUser requestedBy, string trackId);
    }
}
