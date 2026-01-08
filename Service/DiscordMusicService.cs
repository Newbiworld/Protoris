using Discord;
using Protoris.Enum;
using Protoris.Service.Interfaces;
using Victoria;

namespace Protoris.Service
{
    public class DiscordMusicService : IDiscordMusicService
    {
        private readonly IMusicService _musicService;
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly IExceptionService _exceptionService;
        public DiscordMusicService(
            IMusicService musicService,
            LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava,
            IExceptionService exceptionService)
        {
            _musicService = musicService;
            _exceptionService = exceptionService;
            _lava = lava;
        }

        #region General Interaction
        public async Task PlayMusic(IDiscordInteraction interaction,
            IMessageChannel currentMessageChannel,
            string url)
        {
            try
            {
                IGuildUser? user = interaction.User as IGuildUser;
                if (user == null)
                {
                    await interaction.RespondAsync("You don't event exist!", ephemeral: true);
                    return;
                }

                IVoiceChannel? currentVoiceChannel = user.VoiceChannel;
                if (currentVoiceChannel == null)
                {
                    await interaction.RespondAsync("Woah, I can't sing when you're not even in VC!", ephemeral: true);
                    return;
                }

                await interaction.DeferAsync(); // In the off chance the loading takes more than 3 seconds, we defers it
                await _musicService.AddMusic(interaction, currentVoiceChannel, currentMessageChannel, user, url);
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task HandleMusicCommand(IDiscordInteraction interaction, EMusicCommand commandEnum)
        {
            try
            {
                IGuildUser? guildUser = interaction.User as IGuildUser;
                (bool canApplyCommand, string reason) = await CanApplyCommand(guildUser, _lava);

                if (!canApplyCommand)
                {
                    await interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                switch (commandEnum)
                {
                    case EMusicCommand.Stop:
                        await _musicService.Stop(guildUser!.VoiceChannel, guildUser, interaction);
                        break;
                    case EMusicCommand.Skip:
                        await _musicService.Skip(interaction, guildUser!);
                        break;
                    case EMusicCommand.ShowPlaylist:
                        await _musicService.ShowPlaylist(interaction, guildUser!);
                        break;
                    case EMusicCommand.ShowGoTo:
                        await _musicService.ShowGoTo(interaction, guildUser!);
                        break;
                }
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Can't stop, won't stop!");
                _exceptionService.LogException(exception);
            }
        }
        #endregion

        #region  Component Interaction
        public async Task HandleComponentInteraction(IComponentInteraction interaction, string trackId, EMusicInteraction musicInteraction )
        {
            IGuildUser? guildUser = interaction.User as IGuildUser;
            (bool canApplyCommand, string reason) = await CanApplyCommand(guildUser, _lava);

            if (!canApplyCommand)
            {
                await interaction.RespondAsync(reason, ephemeral: true);
                return;
            }

            switch (musicInteraction)
            {
                case EMusicInteraction.Remove:
                    await RemoveSong(interaction, guildUser!, trackId);
                    break;
                case EMusicInteraction.GoToSong:
                    await GoToTrack(interaction, guildUser!, trackId);
                    break;
            }
        }

        private async Task RemoveSong(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ComponentBuilderV2 builder = await _musicService.RemoveSong(guildUser, trackId);
            await interaction.UpdateAsync(m => m.Components = builder.Build());
        }

        private async Task GoToTrack(IComponentInteraction interaction, IGuildUser guildUser, string trackId)
        {
            ComponentBuilderV2 builder = await _musicService.GoToSong(guildUser, trackId);
            await interaction.UpdateAsync(m => m.Components = builder.Build());
        }
        #endregion

        #region Validation
        private async Task<(bool canApplyCommand, string reason)> CanApplyCommand(IGuildUser? guildUser, LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava)
        {
            if (guildUser == null)
            {
                return (false, "I don't know why, but you don't exist?");
            }

            if (guildUser.VoiceChannel == null)
            {
                return (false, "I don't listen to people who ain't with me in vc!");
            }

            LavaPlayer<LavaTrack>? lavaPlayer = await lava.TryGetPlayerAsync(guildUser.GuildId);
            if (lavaPlayer == null)
            {
                return (false, "Wow, don't tell me to do stuff when I ain't even on stage!");
            }

            if (lavaPlayer.Track == null)
            {
                return (false, "Wow, don't tell me what to do when I ain't singing! ... rude");
            }

            return (true, string.Empty);
        }
        #endregion
    }
}
