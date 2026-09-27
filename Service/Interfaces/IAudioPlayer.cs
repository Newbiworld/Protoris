using Discord;
using Protoris.Data;
using Protoris.Events.Data;

namespace Protoris.Service.Interfaces
{
    public interface IAudioPlayer
    {
        public Task JoinAsync(IVoiceChannel voiceChannel);
        public Task<bool> PlayAsync(ulong guildId, TrackInformations trackInfo);
        public Task SkipAsync(ulong guildId);
        public Task StopAsync(ulong guildId, IVoiceChannel voiceChannel);
        public Task<IsInVcResponse> IsInVc(ulong guildId);
        public Task<bool> IsPlaying(ulong guildId);
        public Task CleanUp(ulong guildId, IVoiceChannel? voiceChannel);
        public event EventHandler<MusicStatusChangedArgs>? MusicStatusChanged;
    }
}
