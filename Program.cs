using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Protoris.Clients.Bot;
using Protoris.Middleware;
using Protoris.Service;
using Protoris.Service.Config;
using Protoris.Service.InteractionService;
using Protoris.Service.Interfaces;
using Victoria;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.UseMiddleware<ExceptionHandleMiddleware>();

builder.Services
    .AddSingleton<IMusicService, MusicService>()
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
    .AddHostedService<BotHost>();

string? hasEmotes = Environment.GetEnvironmentVariable("HasEmotes");

if (string.IsNullOrEmpty(hasEmotes) || hasEmotes.ToLower() != "true")
{
    builder.Services
        .AddSingleton<IEmoteService, FakeEmoteService>()
        .AddSingleton<IMusicComponentService, MusicComponentServiceWithoutEmotes>();
}
else
{
    builder.Services
        .AddSingleton<IEmoteService, EmoteService>()
        .AddSingleton<IMusicComponentService, MusicComponentService>();
}

builder.Build().Run();