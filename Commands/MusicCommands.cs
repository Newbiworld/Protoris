using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Protoris.Service;
using Protoris.Service.Interfaces;
using Victoria;

namespace Protoris.Commands
{
    public class MusicCommands : BaseCommands
    {
        private readonly IMusicService _musicService;
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lava;
        private readonly DiscordSocketClient _discordClient;
        private readonly IExceptionService _exceptionService;
        public MusicCommands(
            IMusicService musicService,
            LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
            DiscordSocketClient discordClient,
            IExceptionService exceptionService)
        {
            _musicService = musicService;
            _lava = lavaNode;
            _discordClient = discordClient;
            _exceptionService = exceptionService;
        }

        [SlashCommand(name: "play-lizard", description: "Make the lizard play musics")]
        public async Task MusicPlay(string url)
        {
            try
            {
                IGuildUser? user = GetUser();
                if (user == null)
                {
                    await RespondAsync("You don't event exist!", ephemeral: true);
                    return;
                }

                IVoiceChannel? currentChannel = user.VoiceChannel;
                if (currentChannel == null)
                {
                    await RespondAsync("Woah, I can't sing when you're not even in VC!", ephemeral: true);
                    return;
                }

                IMessageChannel messageChannel = GetInteractionChannel();
                ComponentBuilderV2 response = await _musicService.AddMusic(currentChannel, messageChannel, user, url);
                await RespondAsync(components: response.Build());
            }
            catch (Exception exception)
            {
                await ReplyAsync("Awww man, something's wrong, I couldn't handle this music request!");
                _exceptionService.LogException(exception);
            }

        }

        [SlashCommand(name: "stop-lizard", description: "Make the lizard stop singing!")]
        public async Task MusicStop()
        {
            try
            {
                IGuildUser? user = GetUser();
                (bool canApplyCommand, string reason) = await CanApplyCommand(user, _lava);

                if (!canApplyCommand)
                {
                    await RespondAsync(reason, ephemeral: true);
                    return;
                }

                ComponentBuilderV2 builder = await DiscordComponentHelper.BuildStopResponse(user!, _discordClient);
                await RespondAsync(components: builder.Build());

                await _musicService.Stop(Context.Guild.Id, user!.VoiceChannel);
            }
            catch (Exception exception)
            {
                await ReplyAsync("Can't stop, won't stop!");
                _exceptionService.LogException(exception);
            }
        }

        [SlashCommand(name: "skip-lizard", description: "Make the lizard stop singing!")]
        public async Task MusicSkip()
        {
            try
            {
                IGuildUser? user = GetUser();
                (bool canApplyCommand, string reason) = await CanApplyCommand(user, _lava);

                if (!canApplyCommand)
                {
                    await RespondAsync(reason, ephemeral: true);
                    return;
                }

                ComponentBuilderV2 builder = await DiscordComponentHelper.BuildSkipResponse(user!, _discordClient);
                await RespondAsync(components: builder.Build());

                await _musicService.Skip(Context.Guild.Id);
            }
            catch (Exception exception)
            {
                await ReplyAsync("Ah! Ya tried to skip me, but I fucked up so hard that I couldn't skip the track!");
                _exceptionService.LogException(exception);
            }
        }

        [SlashCommand(name: "playlist-lizard", description: "Show what the lizard's gonna sing!")]
        public async Task MusicPlaylist()
        {
            try
            {
                IGuildUser? user = GetUser();
                (bool canApplyCommand, string reason) = await CanApplyCommand(user, _lava);

                if (!canApplyCommand)
                {
                    await RespondAsync(reason, ephemeral: true);
                    return;
                }

                ComponentBuilderV2 builder = await _musicService.ShowPlaylist(Context.Guild.Id, user!);
                await RespondAsync(components: builder.Build());
            }
            catch (Exception exception)
            {
                await ReplyAsync("I tried to read what's in my queue, but I remember that I can't read!");
                _exceptionService.LogException(exception);
            }
        }

        #region Validation
        public static async Task<(bool canApplyCommand, string reason)> CanApplyCommand(IGuildUser? guildUser, LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lava)
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
