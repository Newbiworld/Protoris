using Discord;
using Discord.WebSocket;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Extensions;
using Protoris.Service.Interfaces;
using Victoria;

namespace Protoris.Service
{
    public class MusicComponentService : IMusicComponentService
    {
        private readonly IEmoteService _emoteService;
        public MusicComponentService(IEmoteService emoteService)
        {
            _emoteService = emoteService;
        }

        public async Task<ComponentBuilderV2> BuildPlayingTrackResponse(IGuildUser botUser, TrackInformations trackInfo, TimeSpan timeSinceStarted)
        {
            Emote coolEzel = _emoteService.EzelCool;
            Emote rightArrow = _emoteService.ArrowRight;
            Emote stop = _emoteService.Stop;

            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            SectionBuilder musicSection = new SectionBuilder();
            musicSection.WithTextDisplay($"### {coolEzel.ToString()} {botUser.GetNicknameOrUsername()} Singing");
            musicSection.WithTextDisplay($"**{currentTrack.Title}** \n[Listen Here]({currentTrack.Url})");
            musicSection.WithTextDisplay($"**Duration** \n{timeSinceStarted.ToString(@"mm\:ss")}/{currentTrack.Duration.ToString(@"mm\:ss")}");

            UnfurledMediaItemProperties thumbnail = new UnfurledMediaItemProperties(currentTrack.Artwork);
            ThumbnailBuilder thumbnailBuilder = new ThumbnailBuilder(thumbnail);
            musicSection.WithAccessory(thumbnailBuilder);

            ContainerBuilder musicContainer = new ContainerBuilder();
            musicContainer.AddComponent(musicSection);
            musicContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            musicContainer.WithAccentColor(Color.Green);

            ContainerBuilder actionContainer = new ContainerBuilder();
            actionContainer.WithActionRow(
            [
                DiscordComponentHelper.CreateButton(stop, InteractionEventEnum.MusicStopped, ButtonStyle.Danger),
                DiscordComponentHelper.CreateButton(rightArrow, InteractionEventEnum.MusicSkipped, ButtonStyle.Primary),
                DiscordComponentHelper.CreateButton("Playlist", $"{InteractionEventEnum.MusicPlaylist}init", ButtonStyle.Primary),
                DiscordComponentHelper.CreateButton("Goto", $"{InteractionEventEnum.MusicGoto}init", ButtonStyle.Primary),
            ]);

            actionContainer.WithAccentColor(Color.DarkerGrey);

            builder.WithContainer(musicContainer);
            builder.WithComponents([musicContainer, actionContainer]);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildAddingTrackResponse(IGuildUser botUser, TrackInformations trackInfo)
        {
            Emote thinkingEzel = _emoteService.EzelThink;

            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            UnfurledMediaItemProperties thumbnail = new UnfurledMediaItemProperties(currentTrack.Artwork);
            ThumbnailBuilder thumbnailBuilder = new ThumbnailBuilder(thumbnail);
            SectionBuilder musicSection = new SectionBuilder();

            musicSection.WithTextDisplay($"### {thinkingEzel.ToString()} {botUser.GetNicknameOrUsername()} Adding");
            musicSection.WithTextDisplay($"**{currentTrack.Title}** \n[Listen Here]({currentTrack.Url})");
            musicSection.WithTextDisplay($"**Duration** \n{currentTrack.Duration.ToString(@"mm\:ss")}");
            musicSection.WithAccessory(thumbnailBuilder);

            ContainerBuilder musicContainer = new ContainerBuilder();
            musicContainer.AddComponent(musicSection);
            musicContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            musicContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(musicContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildAddingTracksResponse(IGuildUser botUser, PlaylistToAddInfo PlaylistInfo)
        {
            Emote thinkingEzel = _emoteService.EzelThink;
            IGuildUser requestedBy = PlaylistInfo.RequestedBy;
            string playlistName = string.IsNullOrEmpty(PlaylistInfo.PlaylistName) ? "Unknown" : PlaylistInfo.PlaylistName;
            TimeSpan totalTime = TimeSpan.Zero;
            PlaylistInfo.PlaylistTracksInfo.ForEach(x =>
            {
                if (x?.Track?.Duration != null)
                {
                    totalTime += x.Track.Duration;
                }
            });

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder musicContainer = new ContainerBuilder();
            musicContainer.WithTextDisplay($"### {thinkingEzel.ToString()} {botUser.GetNicknameOrUsername()} Adding a LOT");
            musicContainer.WithTextDisplay($"**{playlistName}** \n[Listen Here]({PlaylistInfo.PlaylistUrl})");
            musicContainer.WithTextDisplay($"**Number of songs added:** \n{PlaylistInfo.PlaylistTracksInfo.Count}");
            musicContainer.WithTextDisplay($"**Duration** \n{totalTime.ToString(@"hh\:mm\:ss")}"); musicContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            musicContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(musicContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildTrackNotFoundResponse(IGuildUser botUser, IGuildUser requestedBy, string songUrl)
        {
            Emote nervousEzel = _emoteService.EzelNervous;
            ComponentBuilderV2 builder = new ComponentBuilderV2();
            bool isUrl = Uri.TryCreate(songUrl, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            ContainerBuilder notFoundContainer = new ContainerBuilder();
            notFoundContainer.WithTextDisplay($"### {nervousEzel.ToString()} {botUser.GetNicknameOrUsername()} Not Found");

            if (isUrl)
            {
                notFoundContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} couldn't find the song!** \n[Song Url]({songUrl})");
            }
            else
            {
                notFoundContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} couldn't find the song!** \nHere's the not valid Url: {songUrl}");
            }

            notFoundContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");

            notFoundContainer.WithAccentColor(Color.Orange);
            builder.WithContainer(notFoundContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildStopResponse(IGuildUser botUser, IGuildUser requestedBy)
        {
            Emote sadEzel = _emoteService.EzelSad;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder leavingContainer = new ContainerBuilder();
            leavingContainer.WithTextDisplay($"### {sadEzel.ToString()} {botUser.GetNicknameOrUsername()} Leaving");
            leavingContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to leave while singing**");
            leavingContainer.WithTextDisplay($"Rudely stopped by: {requestedBy.GetNicknameOrUsername()}");

            leavingContainer.WithAccentColor(Color.DarkRed);
            builder.WithContainer(leavingContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildSkipResponse(IGuildUser botUser, IGuildUser requestedBy)
        {
            Emote surprisedEzel = _emoteService.EzelSurprised;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder skippingContainer = new ContainerBuilder();
            skippingContainer.WithTextDisplay($"### {surprisedEzel.ToString()} {botUser.GetNicknameOrUsername()} Skipping");
            skippingContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to skip this song!**");
            skippingContainer.WithTextDisplay($"Rudely skipped by: {requestedBy.GetNicknameOrUsername()}");

            skippingContainer.WithAccentColor(Color.DarkOrange);
            builder.WithContainer(skippingContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildRemoveResponse(IGuildUser botUser, IGuildUser requestedBy, LavaTrack? track)
        {
            Emote surprisedEzel = _emoteService.EzelSurprised;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder removeContainer = new ContainerBuilder();
            removeContainer.WithTextDisplay($"### {surprisedEzel.ToString()} {botUser.GetNicknameOrUsername()} Removing");
            removeContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to remove a song!**");

            if (track != null)
            {
                removeContainer.WithTextDisplay($"Removed song: [{track.Title}]({track.Url})");
                removeContainer.WithTextDisplay($"Removed by: {requestedBy.GetNicknameOrUsername()}");
            }
            else
            {
                removeContainer.WithTextDisplay($"but that song was already gone...");
                removeContainer.WithTextDisplay($"Tried by: {requestedBy.GetNicknameOrUsername()}");
            }

            removeContainer.WithAccentColor(Color.DarkOrange);
            builder.WithContainer(removeContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildPlaylistResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations, int index)
        {
            Emote thinkingHardEzel = _emoteService.EzelThinkWithCloud;
            Emote delete = _emoteService.Bin;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {thinkingHardEzel.ToString()} {botUser.GetNicknameOrUsername()} Playlist");
            playlistContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to show his playlist!**");

            TrackInformationsTable tracksTable = new TrackInformationsTable(trackInformations,
                index,
                InteractionEventEnum.MusicPlaylist,
                InteractionEventEnum.MusicRemoved);

            tracksTable.ApplyTableToContainer(playlistContainer, (string buttonId) => DiscordComponentHelper.CreateButton(delete, buttonId, ButtonStyle.Danger));

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildGoToResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations, int index)
        {
            Emote thinkingHardEzel = _emoteService.EzelThinkWithCloud;
            Emote arrowEmote = _emoteService.ArrowRight;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {thinkingHardEzel.ToString()} {botUser.GetNicknameOrUsername()} GoTo");
            playlistContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked skip to a song!**");

            TrackInformationsTable tracksTable = new TrackInformationsTable(trackInformations,
                index,
                InteractionEventEnum.MusicGoto,
                InteractionEventEnum.MusicGotoButton);

            tracksTable.ApplyTableToContainer(playlistContainer, (string buttonId) => DiscordComponentHelper.CreateButton(arrowEmote, buttonId, ButtonStyle.Danger));

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildFarewellResponse(IGuildUser botUser)
        {
            Emote sleepingEzel = _emoteService.EzelSleep;
            Emote heartEzel = _emoteService.EzelHeart;
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder farewellContainer = new ContainerBuilder();
            farewellContainer.WithTextDisplay($"### {sleepingEzel.ToString()} {botUser.GetNicknameOrUsername()} sleeping");
            farewellContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} sang his heart to y'all, now it's time for a nap**");
            farewellContainer.WithTextDisplay($"Don't worry, he'll be back soon to sing more to you {heartEzel.ToString()}");
            farewellContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(farewellContainer);
            return builder;
        }
    }
}
