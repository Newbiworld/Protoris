using Discord.Interactions;
using Protoris.Enum;
using Protoris.Service.Interfaces;

namespace Protoris.Commands
{
    public class MusicCommands : BaseCommands
    {
        private readonly IDiscordMusicService _discordMusicService;
        public MusicCommands(IDiscordMusicService discordMusicService)
        {
            _discordMusicService = discordMusicService;
        }

        [SlashCommand(name: "play-lizard", description: "Make the lizard play musics")]
        public async Task PlayMusicCommand(string url)
        {
            await _discordMusicService.PlayMusic(Context, Context.Channel, url);
        }

        [SlashCommand(name: "stop-lizard", description: "Make the lizard stop singing!")]
        public async Task StopMusicCommand()
        {
            await _discordMusicService.HandleMusicCommand(Context.Interaction, EMusicCommand.Stop);
        }

        [SlashCommand(name: "skip-lizard", description: "Make the lizard stop singing!")]
        public async Task SkipMusicCommand()
        {
            await _discordMusicService.HandleMusicCommand(Context.Interaction, EMusicCommand.Skip);

        }

        [SlashCommand(name: "playlist-lizard", description: "Show what the lizard's gonna sing!")]
        public async Task ShowPlaylistCommand()
        {
            await _discordMusicService.HandleMusicCommand(Context.Interaction, EMusicCommand.ShowPlaylist);
        }

        [SlashCommand(name: "goto-lizard", description: "Skip to said song!")]
        public async Task ShowGoToCommand()
        {
            await _discordMusicService.HandleMusicCommand(Context.Interaction, EMusicCommand.ShowGoTo);
        }
    }
}
