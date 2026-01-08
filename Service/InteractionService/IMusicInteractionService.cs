using Discord.WebSocket;

namespace Protoris.Service.InteractionService
{
    public interface IMusicInteractionService
    {
        public Task ApplyCorrectResponse(string buttonId, SocketMessageComponent component);
    }
}
