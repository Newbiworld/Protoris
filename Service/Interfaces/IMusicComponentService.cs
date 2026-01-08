using Discord;
using Protoris.Data;
using Victoria;

namespace Protoris.Service.Interfaces
{
    public interface IMusicComponentService
    {
        public Task<ComponentBuilderV2> BuildPlayingTrackResponse(TrackInformations trackInfo, TimeSpan timeSinceStarted);
        public Task<ComponentBuilderV2> BuildAddingTrackResponse(TrackInformations trackInfo);
        public Task<ComponentBuilderV2> BuildTrackNotFoundResponse(IGuildUser requestedBy, string songUrl);
        public Task<ComponentBuilderV2> BuildStopResponse(IGuildUser requestedBy);
        public Task<ComponentBuilderV2> BuildSkipResponse(IGuildUser requestedBy);
        public Task<ComponentBuilderV2> BuildRemoveResponse(IGuildUser requestedBy, LavaTrack? track);
        public Task<ComponentBuilderV2> BuildPlaylistResponse(IGuildUser requestedBy, List<TrackInformations> trackInformations);
        public Task<ComponentBuilderV2> BuildGoToResponse(IGuildUser requestedBy, List<TrackInformations> trackInformations);
        public Task<ComponentBuilderV2> BuildFarewellResponse();
    }
}
