using Discord.WebSocket;

namespace Protoris.Service.InteractionService
{
    public interface IMusicInteractionService
    {
        public Task SkipSong(SocketMessageComponent component);
        public Task StopSong(SocketMessageComponent component);
        public Task RemoveSong(SocketMessageComponent component, string trackId);
        public Task ShowPlaylist(SocketMessageComponent component);
        public Task ShowGoTo(SocketMessageComponent component);
        public Task GoTo(SocketMessageComponent component, string trackId);
    }
}
