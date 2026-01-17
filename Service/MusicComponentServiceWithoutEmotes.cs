using Discord;
using Discord.WebSocket;
using Protoris.Data;
using Protoris.Enum;
using Protoris.Extensions;
using Protoris.Service.Interfaces;
using Victoria;

namespace Protoris.Service
{
    public class MusicComponentServiceWithoutEmotes : IMusicComponentService
    {
        public async Task<ComponentBuilderV2> BuildPlayingTrackResponse(IGuildUser botUser, TrackInformations trackInfo, TimeSpan timeSinceStarted)
        {
            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            SectionBuilder musicSection = new SectionBuilder();
            musicSection.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Singing");
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
                CreateButton("Stop", InteractionEventEnum.MusicStopped, ButtonStyle.Danger),
                CreateButton("Skip", InteractionEventEnum.MusicSkipped, ButtonStyle.Primary),
                CreateButton("Playlist", InteractionEventEnum.MusicPlaylist, ButtonStyle.Primary),
                CreateButton("Goto", InteractionEventEnum.MusicGoto, ButtonStyle.Primary),
            ]);

            actionContainer.WithAccentColor(Color.DarkerGrey);

            builder.WithContainer(musicContainer);
            builder.WithComponents([musicContainer, actionContainer]);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildAddingTrackResponse(IGuildUser botUser, TrackInformations trackInfo)
        {
            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            UnfurledMediaItemProperties thumbnail = new UnfurledMediaItemProperties(currentTrack.Artwork);
            ThumbnailBuilder thumbnailBuilder = new ThumbnailBuilder(thumbnail);
            SectionBuilder musicSection = new SectionBuilder();

            musicSection.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Adding");
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

        public async Task<ComponentBuilderV2> BuildTrackNotFoundResponse(IGuildUser botUser, IGuildUser requestedBy, string songUrl)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();
            bool isUrl = Uri.TryCreate(songUrl, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            ContainerBuilder notFoundContainer = new ContainerBuilder();
            notFoundContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Not Found");

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
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder leavingContainer = new ContainerBuilder();
            leavingContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Leaving");
            leavingContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to leave while singing**");
            leavingContainer.WithTextDisplay($"Rudely stopped by: {requestedBy.GetNicknameOrUsername()}");

            leavingContainer.WithAccentColor(Color.DarkRed);
            builder.WithContainer(leavingContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildSkipResponse(IGuildUser botUser, IGuildUser requestedBy)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder skippingContainer = new ContainerBuilder();
            skippingContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Skipping");
            skippingContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to skip this song!**");
            skippingContainer.WithTextDisplay($"Rudely skipped by: {requestedBy.GetNicknameOrUsername()}");

            skippingContainer.WithAccentColor(Color.DarkOrange);
            builder.WithContainer(skippingContainer);

            return builder;
        }

        public async Task<ComponentBuilderV2> BuildRemoveResponse(IGuildUser botUser, IGuildUser requestedBy, LavaTrack? track)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder removeContainer = new ContainerBuilder();
            removeContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Removing");
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

        public async Task<ComponentBuilderV2> BuildPlaylistResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} Playlist");
            playlistContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked to show his playlist!**");

            if (!trackInformations.Any())
            {
                playlistContainer.WithTextDisplay($"But he had nothing to show");
            }
            else
            {
                for (int i = 1; i < trackInformations.Count + 1; i++)
                {
                    TrackInformations trackInfo = trackInformations[i - 1];
                    LavaTrack track = trackInfo.Track;
                    string id = trackInfo.Id;

                    SectionBuilder trackSection = new SectionBuilder();
                    trackSection.WithAccessory(CreateButton("Delete", $"{InteractionEventEnum.MusicRemoved}{id}", ButtonStyle.Danger));
                    trackSection.WithTextDisplay($"{i}. [{track.Title}]({track.Url}) | Duration: {track.Duration.ToString(@"mm\:ss")}");
                    playlistContainer.AddComponent(trackSection);
                }
            }

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildGoToResponse(IGuildUser botUser, IGuildUser requestedBy, List<TrackInformations> trackInformations)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} GoTo");
            playlistContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} was asked skip to a song!**");

            if (!trackInformations.Any())
            {
                playlistContainer.WithTextDisplay($"But he had nothing left to sing");
            }
            else
            {
                for (int i = 1; i < trackInformations.Count + 1; i++)
                {
                    TrackInformations trackInfo = trackInformations[i - 1];
                    LavaTrack track = trackInfo.Track;
                    string id = trackInfo.Id;

                    SectionBuilder trackSection = new SectionBuilder();
                    trackSection.WithAccessory(CreateButton("Goto", $"{InteractionEventEnum.MusicGotoButton}{id}", ButtonStyle.Danger));
                    trackSection.WithTextDisplay($"{i}. [{track.Title}]({track.Url}) | Duration: {track.Duration.ToString(@"mm\:ss")}");
                    playlistContainer.AddComponent(trackSection);
                }
            }

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.GetNicknameOrUsername()}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        public async Task<ComponentBuilderV2> BuildFarewellResponse(IGuildUser botUser)
        {
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder farewellContainer = new ContainerBuilder();
            farewellContainer.WithTextDisplay($"### {botUser.GetNicknameOrUsername()} sleeping");
            farewellContainer.WithTextDisplay($"**{botUser.GetNicknameOrUsername()} sang his heart to y'all, now it's time for a nap**");
            farewellContainer.WithTextDisplay($"Don't worry, he'll be back soon to sing more to you");
            farewellContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(farewellContainer);
            return builder;
        }

        #region Helper
        private ButtonBuilder CreateButton(Emote emote, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithEmote(emote);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }

        private ButtonBuilder CreateButton(string label, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithLabel(label);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }
        #endregion
    }
}
