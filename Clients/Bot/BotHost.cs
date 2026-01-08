using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Protoris.Enum;
using Protoris.Service.InteractionService;
using Protoris.Service.Interfaces;
using System.Reflection;
using Victoria;

namespace Protoris.Clients.Bot
{
    public class BotHost : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly IBotConfig _botConfig;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _servicesProviders;
        private readonly IMusicInteractionService _musicInteractionService;
        private readonly IMusicService _musicService;
        private readonly IEmoteService _emoteService;

        public BotHost(DiscordSocketClient client,
            IBotConfig botConfig,
            IServiceProvider serviceProvider,
            IMusicService musicService,
            IEmoteService emoteService,
            IMusicInteractionService musicInteractionService)
        {
            _client = client;
            _botConfig = botConfig;
            _interactionService = new InteractionService(client.Rest);
            _servicesProviders = serviceProvider;
            _musicService = musicService;
            _musicInteractionService = musicInteractionService;
            _emoteService = emoteService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.LoginAsync(TokenType.Bot, _botConfig.TokenKey);
            _client.Log += Log;
            _client.Ready += RegisterCommands;
            _client.InteractionCreated += HandleInteraction;
            _client.UserVoiceStateUpdated += HandleVoiceUserUpdated;
            _client.ButtonExecuted += HandleOnButtonClicked;

            await _client.StartAsync();
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                SocketInteractionContext? ctx = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, _servicesProviders);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task RegisterCommands()
        {
            await _emoteService.LoadEmotesAsync(_client);

            await _servicesProviders.UseLavaNodeAsync();

            // Add modules
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _servicesProviders);

            // Check if testing in prod or dev

            if (_botConfig.IsBetaTesting)
            {
                await _interactionService.RegisterCommandsToGuildAsync(ulong.Parse(_botConfig.TestingServerId));
            }
            else
            {
                await _interactionService.RegisterCommandsGloballyAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean everything
            await _client.StopAsync();
        }

        private async Task HandleVoiceUserUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.Id != _client.CurrentUser.Id)
            {
                return; // It's not our bot, not our job to work with it
            }

            if (before.VoiceChannel != null && after.VoiceChannel == null)
            {
                ulong? guildId = before.VoiceChannel?.Guild?.Id;
                if (guildId != null)
                {
                    await _musicService.Stop(before.VoiceChannel!, user as IGuildUser);
                }
            }
        }

        private async Task HandleOnButtonClicked(SocketInteraction arg)
        {
            switch (arg)
            {
                case SocketMessageComponent component:
                    switch (component.Data.CustomId)
                    {
                        case { } when component.Data.CustomId.StartsWith("Music"):
                            await _musicInteractionService.ApplyCorrectResponse(component.Data.CustomId, component);
                            break;

                        default:
                            return;
                    }
                    break;
                default:
                    return;
            }
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
