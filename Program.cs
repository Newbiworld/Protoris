using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Protoris.Clients.Bot;
using Protoris.Events;
using Protoris.Middleware;
using Protoris.Service;
using Protoris.Service.Config;
using Protoris.Service.InteractionService;
using Protoris.Service.Interfaces;
using Protoris.Service.TrackResolver;
using Victoria;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.UseMiddleware<ExceptionHandleMiddleware>();

builder.Services
    .AddSingleton<IMusicPlaylistService, MusicPaylistService>()
    .AddSingleton<IFileConfig, FileConfig>()
    .AddSingleton<IExceptionService, ExceptionService>()
    .AddSingleton<IMusicInteractionService, MusicInteractionService>()
    .AddSingleton<IDiscordMusicService, DiscordMusicService>()
    .AddHttpClient()
    .AddLavaNode(config =>
    {
        config.Hostname = "127.0.0.1";
        config.Port = 2333;
        config.Authorization = "ezelprotoris!";
        config.SelfDeaf = true;
    })
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton(new DiscordSocketClient())
    .AddSingleton<IBotConfig, BotConfig>()
    .AddSingleton<ISpotifyConfig, SpotifyConfig>()
    .AddSingleton<IDiscordContextProvider, DiscordContextProvider>()
    .AddSingleton<IMasterTrackResolver, MasterTrackResolver>()
    .AddSingleton<ITrackResolver, YoutubeTrackResolver>()
    .AddSingleton<ITrackResolver, SpotifyTrackResolver>()
    .AddSingleton<ITrackResolver, LavalinkTrackResolver>()
    .AddSingleton<IAudioPlayer, LavaLinkAudioPlayer>()
    .AddSingleton<ITrackPreloader, LavaLinkTrackPreLoader>()
    .AddHostedService<MusicEventHandler>() // Probably not catholic
    .AddHostedService<BotHost>()
    .AddSingleton<IEmoteService, EmoteService>()
    .AddSingleton<IMusicComponentService, MusicComponentService>();

builder.Build().Run();