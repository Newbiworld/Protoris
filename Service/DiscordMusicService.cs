using Discord;
using Discord.WebSocket;
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

        public async Task StopMusic(IDiscordInteraction interaction)
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

                ComponentBuilderV2 builder = await _musicService.Stop(guildUser!.GuildId, guildUser!.VoiceChannel, guildUser);
                await interaction.RespondAsync(components: builder.Build());

            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Can't stop, won't stop!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task SkipMusic(IDiscordInteraction interaction)
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

                ComponentBuilderV2 builder = await _musicService.Skip(guildUser!.GuildId, guildUser);
                await interaction.RespondAsync(components: builder.Build());
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("Ah! Ya tried to skip me, but I fucked up so hard that I couldn't skip the track!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task ShowPlaylist(IDiscordInteraction interaction)
        {
            try
            {
                IGuildUser? user = interaction.User as IGuildUser;
                (bool canApplyCommand, string reason) = await CanApplyCommand(user, _lava);

                if (!canApplyCommand)
                {
                    await interaction.RespondAsync(reason, ephemeral: true);
                    return;
                }

                ComponentBuilderV2 builder = await _musicService.ShowPlaylist(user!.GuildId, user!);
                await interaction.RespondAsync(components: builder.Build());
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("I tried to read what's in my queue, but I remember that I can't read!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task RemoveSong(IComponentInteraction interaction, string trackId)
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

                ComponentBuilderV2 builder = await _musicService.RemoveSong(guildUser!.GuildId, guildUser, trackId);
                await interaction.UpdateAsync(m => m.Components = builder.Build());
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("I tried to remove that song, but well...");
                _exceptionService.LogException(exception);
            }
        }

        public async Task ShowGoTo(IDiscordInteraction interaction)
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

                ComponentBuilderV2 builder = await _musicService.ShowGoTo(guildUser!.GuildId, guildUser);
                await interaction.RespondAsync(components: builder.Build(), ephemeral: true);
            }
            catch(Exception exception)
            {
                await interaction.RespondAsync("AH! Ya can't go to!");
                _exceptionService.LogException(exception);
            }
        }

        public async Task GoToTrack(IComponentInteraction interaction, string trackId)
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

                await _musicService.GoToSong(guildUser!.GuildId, guildUser, trackId);

                ComponentBuilderV2 builder = await _musicService.ShowGoTo(guildUser.GuildId, guildUser);
                await interaction.UpdateAsync(m => m.Components = builder.Build());
            }
            catch (Exception exception)
            {
                await interaction.RespondAsync("AH! Ya can't go to!");
                _exceptionService.LogException(exception);
            }
        }

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
