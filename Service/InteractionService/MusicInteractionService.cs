using Discord;
using Discord.WebSocket;
using Protoris.Commands;
using Protoris.Service.Interfaces;
using Victoria;

namespace Protoris.Service.InteractionService
{
    public class MusicInteractionService : IMusicInteractionService
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly DiscordSocketClient _dicordClient;
        private readonly IMusicService _musicService;
        private readonly IExceptionService _exceptionService;

        public MusicInteractionService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava,
            DiscordSocketClient dicordClient,
            IMusicService musicService,
            IExceptionService exceptionService)
        {
            _lava = lava;
            _musicService = musicService;
            _dicordClient = dicordClient;
            _exceptionService = exceptionService;
        }

        public async Task SkipSong(SocketMessageComponent component)
        {
            try
            {
                SocketUser user = component.User;
                if (user is not IGuildUser)
                {
                    await component.RespondAsync("Y-you're not real???", ephemeral: true);
                    return;
                }

                IGuildUser guildUser = (user as IGuildUser)!;
                (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
                if (!canApplyInteraction)
                {
                    await component.RespondAsync(reason, ephemeral: true);
                    return;
                }

                ComponentBuilderV2 builder = await DiscordComponentHelper.BuildSkipResponse(guildUser!, _dicordClient);
                await component.RespondAsync(components: builder.Build());

                await _musicService.Skip(guildUser.GuildId);
            }
            catch (Exception ex)
            {
                await component.RespondAsync("For real, I tried to skip the song, but well, nah.", ephemeral: true);
                _exceptionService.LogException(ex);
            }
        }

        public async Task StopSong(SocketMessageComponent component)
        {
            SocketUser user = component.User;
            if (user is not IGuildUser)
            {
                await component.RespondAsync("Y-you don't exist???", ephemeral: true);
                return;
            }

            IGuildUser guildUser = (user as IGuildUser)!;
            (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
            if (!canApplyInteraction)
            {
                await component.RespondAsync(reason, ephemeral: true);
                return;
            }

            ComponentBuilderV2 builder = await DiscordComponentHelper.BuildStopResponse(guildUser!, _dicordClient);
            await component.RespondAsync(components: builder.Build());

            await _musicService.Stop(guildUser.GuildId, guildUser.VoiceChannel);
        }

        public async Task RemoveSong(SocketMessageComponent component, string trackId)
        {
            SocketUser user = component.User;
            if (user is not IGuildUser)
            {
                await component.RespondAsync("You're a ghost???", ephemeral: true);
                return;
            }

            IGuildUser guildUser = (user as IGuildUser)!;
            (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
            if (!canApplyInteraction)
            {
                await component.RespondAsync(reason, ephemeral: true);
                return;
            }

            _musicService.RemoveSong(guildUser.GuildId, guildUser, trackId);
            
            ComponentBuilderV2 builder = await _musicService.ShowPlaylist(guildUser.GuildId, guildUser);
            await component.UpdateAsync(m => m.Components = builder.Build());
        }

        public async Task ShowPlaylist(SocketMessageComponent component)
        {
            SocketUser user = component.User;
            if (user is not IGuildUser)
            {
                await component.RespondAsync("You're a ghost???", ephemeral: true);
                return;
            }

            IGuildUser guildUser = (user as IGuildUser)!;
            (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
            if (!canApplyInteraction)
            {
                await component.RespondAsync(reason, ephemeral: true);
                return;
            }

            ComponentBuilderV2 builder = await _musicService.ShowPlaylist(guildUser.GuildId, guildUser);
            await component.RespondAsync(components: builder.Build(), ephemeral: true);
        }

        public async Task ShowGoTo(SocketMessageComponent component)
        {
            SocketUser user = component.User;
            if (user is not IGuildUser)
            {
                await component.RespondAsync("I can't believe you ain't real!!", ephemeral: true);
                return;
            }

            IGuildUser guildUser = (user as IGuildUser)!;
            (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
            if (!canApplyInteraction)
            {
                await component.RespondAsync(reason, ephemeral: true);
                return;
            }

            ComponentBuilderV2 builder = await _musicService.ShowGoTo(guildUser.GuildId, guildUser);
            await component.RespondAsync(components: builder.Build(), ephemeral: true);
        }

        public async Task GoTo(SocketMessageComponent component, string trackId)
        {
            SocketUser user = component.User;
            if (user is not IGuildUser)
            {
                await component.RespondAsync("You fake!", ephemeral: true);
                return;
            }

            IGuildUser guildUser = (user as IGuildUser)!;
            (bool canApplyInteraction, string reason) = await MusicCommands.CanApplyCommand(guildUser, _lava);
            if (!canApplyInteraction)
            {
                await component.RespondAsync(reason, ephemeral: true);
                return;
            }

            await _musicService.GoToSong(guildUser.GuildId, guildUser, trackId);

            ComponentBuilderV2 builder = await _musicService.ShowGoTo(guildUser.GuildId, guildUser);
            await component.UpdateAsync(m => m.Components = builder.Build());
        }
    }
}
