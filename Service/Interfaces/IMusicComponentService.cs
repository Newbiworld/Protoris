using Discord;
using Protoris.Data;
using Victoria;

namespace Protoris.Service.Interfaces
{
    public interface IMusicComponentService
    {
        public Task<ComponentBuilderV2> BuildPlayingTrackResponse(IGuildUser botUser, TrackInformations trackInfo, TimeSpan timeSinceStarted);
        public Task<ComponentBuilderV2> BuildAddingTrackResponse(IGuildUser botUser, TrackInformations trackInfo);
        public Task<ComponentBuilderV2> BuildTrackNotFoundResponse(IGuildUser botUser, IGuildUser requestedBy, string songUrl);
        public Task<ComponentBuilderV2> BuildStopResponse(IGuildUser botUser, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> BuildSkipResponse(IGuildUser botUser, IGuildUser requestedBy);
        public Task<ComponentBuilderV2> BuildRemoveResponse(IGuildUser botUser, IGuildUser requestedBy, LavaTrack? track);
        public Task<ComponentBuilderV2> BuildPlaylistResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations);
        public Task<ComponentBuilderV2> BuildGoToResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations);
        public Task<ComponentBuilderV2> BuildFarewellResponse(IGuildUser botUser);
    }
}
