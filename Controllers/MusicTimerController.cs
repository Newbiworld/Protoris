using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Protoris.Middleware;
using Protoris.Service.Config;
using Protoris.Service.Interfaces;

namespace Protoris.NewFolder
{
    public class MusicTimerController : ExceptionHandleMiddleware
    {
        private readonly IMusicService _musicService;
        public MusicTimerController(IMusicService musicService, IFileConfig fileConfig) : base(fileConfig)
        {
            _musicService = musicService;
        }

        [Function(nameof(UpdateCurrentPlayingSongTimer))]
        public async Task UpdateCurrentPlayingSongTimer([TimerTrigger("*/2 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            await _musicService.UpdateCurrentlyPlayingSongs();
        }
    }
}
