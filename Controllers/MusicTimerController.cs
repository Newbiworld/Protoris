using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Protoris.Middleware;
using Protoris.Service.Config;
using Protoris.Service.Interfaces;

namespace Protoris.Controllers
{
    public class MusicTimerController : ExceptionHandleMiddleware
    {
        private readonly IDiscordMusicService _discordMusicService;
        public MusicTimerController(IDiscordMusicService discordMusicService, IFileConfig fileConfig) : base(fileConfig)
        {
            _discordMusicService = discordMusicService;
        }

        [Function(nameof(UpdateCurrentPlayingSongTimer))]
        public async Task UpdateCurrentPlayingSongTimer([TimerTrigger("* * * * * *")] TimerInfo myTimer, ILogger log)
        {
            await _discordMusicService.UpdateCurrentlyPlayingSongs();
        }
    }
}
