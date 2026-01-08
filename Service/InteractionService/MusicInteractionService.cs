using Discord.WebSocket;
using Protoris.Enum;
using Protoris.Service.Interfaces;

namespace Protoris.Service.InteractionService
{
    public class MusicInteractionService : IMusicInteractionService
    {
        private readonly IDiscordMusicService _discordMusicService;

        public MusicInteractionService(IDiscordMusicService discordMusicService)
        {
            _discordMusicService = discordMusicService;
        }

        public async Task ApplyCorrectResponse(string buttonId, SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case InteractionEventEnum.MusicSkipped:
                    await _discordMusicService.HandleMusicCommand(component, EMusicCommand.Skip);
                    break;

                case InteractionEventEnum.MusicStopped:
                    await _discordMusicService.HandleMusicCommand(component, EMusicCommand.Stop);
                    break;

                case InteractionEventEnum.MusicPlaylist:
                    await _discordMusicService.HandleMusicCommand(component, EMusicCommand.ShowPlaylist);
                    break;

                case InteractionEventEnum.MusicGoto:
                    await _discordMusicService.HandleMusicCommand(component, EMusicCommand.ShowGoTo);
                    break;

                case { } when component.Data.CustomId.StartsWith(InteractionEventEnum.MusicRemoved):
                    string trackIdToRemove = component.Data.CustomId.Replace(InteractionEventEnum.MusicRemoved, string.Empty);
                    await _discordMusicService.HandleComponentInteraction(component, trackIdToRemove, EMusicInteraction.Remove);
                    break;

                case { } when component.Data.CustomId.StartsWith(InteractionEventEnum.MusicGotoButton):
                    string trackIdToGoTo = component.Data.CustomId.Replace(InteractionEventEnum.MusicGotoButton, string.Empty);
                    await _discordMusicService.HandleComponentInteraction(component, trackIdToGoTo, EMusicInteraction.GoToSong);
                    break;
            }
        }
    }
}
