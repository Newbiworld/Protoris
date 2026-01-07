using Discord;
using Discord.WebSocket;
using Protoris.Data;
using Protoris.Enum;
using Victoria;

namespace Protoris.Service
{
    public class DiscordComponentHelper
    {
        public static async Task<ComponentBuilderV2> BuildPlayingTrackResponse(TrackInformations trackInfo, DiscordSocketClient client)
        {
            Emote coolEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.CoolEzel);
            Emote rightArrow = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.RightArrow);
            Emote stop = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.Stop);

            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            SectionBuilder musicSection = new SectionBuilder();
            musicSection.WithTextDisplay($"### {coolEzel.ToString()} Protoris Singing");
            musicSection.WithTextDisplay($"**{currentTrack.Title}** \n[Listen Here]({currentTrack.Url})");
            musicSection.WithTextDisplay($"**Duration** \n{currentTrack.Duration.ToString(@"mm\:ss")}");

            UnfurledMediaItemProperties thumbnail = new UnfurledMediaItemProperties(currentTrack.Artwork);
            ThumbnailBuilder thumbnailBuilder = new ThumbnailBuilder(thumbnail);
            musicSection.WithAccessory(thumbnailBuilder);

            ContainerBuilder musicContainer = new ContainerBuilder();
            musicContainer.AddComponent(musicSection);
            musicContainer.WithTextDisplay($"Requested by: {requestedBy.Nickname ?? requestedBy.Username}");
            musicContainer.WithAccentColor(Color.Green);

            ContainerBuilder actionContainer = new ContainerBuilder();
            actionContainer.WithActionRow(
            [
                CreateButton(stop, InteractionEventEnum.MusicStopped, ButtonStyle.Danger),
                CreateButton(rightArrow, InteractionEventEnum.MusicSkipped, ButtonStyle.Primary),
                CreateButton("Playlist", InteractionEventEnum.MusicPlaylist, ButtonStyle.Primary),
                CreateButton("Goto", InteractionEventEnum.MusicGoto, ButtonStyle.Primary),
            ]);

            actionContainer.WithAccentColor(Color.DarkerGrey);

            builder.WithContainer(musicContainer);
            builder.WithComponents([musicContainer, actionContainer]);
            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildAddingTrackResponse(TrackInformations trackInfo, DiscordSocketClient client)
        {
            Emote thinkingEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.ThinkingEzel);

            LavaTrack currentTrack = trackInfo.Track;
            IGuildUser requestedBy = trackInfo.RequestedBy;

            ComponentBuilderV2 builder = new ComponentBuilderV2();

            UnfurledMediaItemProperties thumbnail = new UnfurledMediaItemProperties(currentTrack.Artwork);
            ThumbnailBuilder thumbnailBuilder = new ThumbnailBuilder(thumbnail);
            SectionBuilder musicSection = new SectionBuilder();

            musicSection.WithTextDisplay($"### {thinkingEzel.ToString()} Protoris Adding");
            musicSection.WithTextDisplay($"**{currentTrack.Title}** \n[Listen Here]({currentTrack.Url})");
            musicSection.WithTextDisplay($"**Duration** \n{currentTrack.Duration.ToString(@"mm\:ss")}");
            musicSection.WithAccessory(thumbnailBuilder);

            ContainerBuilder musicContainer = new ContainerBuilder();
            musicContainer.AddComponent(musicSection);
            musicContainer.WithTextDisplay($"Requested by: {requestedBy.Nickname ?? requestedBy.Username}");
            musicContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(musicContainer);

            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildTrackNotFoundResponse(IGuildUser requestedBy, string songUrl, DiscordSocketClient client)
        {
            Emote nervousEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.NervousEzel);
            ComponentBuilderV2 builder = new ComponentBuilderV2();
            bool isUrl = Uri.TryCreate(songUrl, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            ContainerBuilder notFoundContainer = new ContainerBuilder();
            notFoundContainer.WithTextDisplay($"### {nervousEzel.ToString()} Protoris Not Found");

            if (isUrl)
            {
                notFoundContainer.WithTextDisplay($"**Protoris couldn't find the song!** \n[Song Url]({songUrl})");
            }
            else
            {
                notFoundContainer.WithTextDisplay($"**Protoris couldn't find the song!** \nHere's the not valid Url: {songUrl}");
            }

            notFoundContainer.WithTextDisplay($"Requested by: {requestedBy.Nickname ?? requestedBy.Username}");

            notFoundContainer.WithAccentColor(Color.Orange);
            builder.WithContainer(notFoundContainer);

            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildStopResponse(IGuildUser requestedBy, DiscordSocketClient client)
        {
            Emote sadEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.SadEzel);
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder leavingContainer = new ContainerBuilder();
            leavingContainer.WithTextDisplay($"### {sadEzel.ToString()} Protoris Leaving");
            leavingContainer.WithTextDisplay($"**Protoris was asked to leave while singing**");
            leavingContainer.WithTextDisplay($"Rudely stopped by: {requestedBy.Nickname ?? requestedBy.Username}");

            leavingContainer.WithAccentColor(Color.DarkRed);
            builder.WithContainer(leavingContainer);

            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildSkipResponse(IGuildUser requestedBy, DiscordSocketClient client)
        {
            Emote surprisedEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.SurprisedEzel);
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder skippingContainer = new ContainerBuilder();
            skippingContainer.WithTextDisplay($"### {surprisedEzel.ToString()} Protoris Skipping");
            skippingContainer.WithTextDisplay($"**Protoris was asked to skip this song!**");
            skippingContainer.WithTextDisplay($"Rudely skipped by: {requestedBy.Nickname ?? requestedBy.Username}");

            skippingContainer.WithAccentColor(Color.DarkOrange);
            builder.WithContainer(skippingContainer);

            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildRemoveResponse(IGuildUser requestedBy, DiscordSocketClient client, LavaTrack? track)
        {
            Emote surprisedEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.SurprisedEzel);
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder removeContainer = new ContainerBuilder();
            removeContainer.WithTextDisplay($"### {surprisedEzel.ToString()} Protoris Removing");
            removeContainer.WithTextDisplay($"**Protoris was asked to remove a song!**");

            if (track != null)
            {
                removeContainer.WithTextDisplay($"Removed song: [{track.Title}]({track.Url})");
                removeContainer.WithTextDisplay($"Removed by: {requestedBy.Nickname ?? requestedBy.Username}");
            }
            else
            {
                removeContainer.WithTextDisplay($"but that song was already gone...");
                removeContainer.WithTextDisplay($"Tried by: {requestedBy.Nickname ?? requestedBy.Username}");
            }

            removeContainer.WithAccentColor(Color.DarkOrange);
            builder.WithContainer(removeContainer);

            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildPlaylistResponse(IGuildUser requestedBy,
            List<TrackInformations> trackInformations,
            DiscordSocketClient client
            )
        {
            Emote thinkingHardEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.ThinkingHardEzel);
            Emote delete = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.Delete);
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {thinkingHardEzel.ToString()} Protoris Playlist");
            playlistContainer.WithTextDisplay($"**Protoris was asked to show his playlist!**");

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
                    trackSection.WithAccessory(CreateButton(delete, $"{InteractionEventEnum.MusicRemoved}{id}", ButtonStyle.Danger));
                    trackSection.WithTextDisplay($"{i}. [{track.Title}]({track.Url}) | Duration: {track.Duration.ToString(@"mm\:ss")}");
                    playlistContainer.AddComponent(trackSection);
                }
            }

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.Nickname ?? requestedBy.Username}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        public static async Task<ComponentBuilderV2> BuildGoToResponse(IGuildUser requestedBy,
            List<TrackInformations> trackInformations,
            DiscordSocketClient client
        )
        {
            Emote thinkingHardEzel = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.ThinkingHardEzel);
            Emote arrowEmote = await client.GetApplicationEmoteAsync(ApplicationEmoteCodeEnum.RightArrow);
            ComponentBuilderV2 builder = new ComponentBuilderV2();

            ContainerBuilder playlistContainer = new ContainerBuilder();
            playlistContainer.WithTextDisplay($"### {thinkingHardEzel.ToString()} Protoris GoTo");
            playlistContainer.WithTextDisplay($"**Protoris was asked skip to a song!**");

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
                    trackSection.WithAccessory(CreateButton(arrowEmote, $"{InteractionEventEnum.MusicGotoButton}{id}", ButtonStyle.Danger));
                    trackSection.WithTextDisplay($"{i}. [{track.Title}]({track.Url}) | Duration: {track.Duration.ToString(@"mm\:ss")}");
                    playlistContainer.AddComponent(trackSection);
                }
            }

            playlistContainer.WithTextDisplay($"Requested by: {requestedBy.Nickname ?? requestedBy.Username}");
            playlistContainer.WithAccentColor(Color.Blue);
            builder.WithContainer(playlistContainer);
            return builder;
        }

        private static ButtonBuilder CreateButton(Emote emote, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithEmote(emote);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }

        private static ButtonBuilder CreateButton(string label, string id, ButtonStyle style)
        {
            ButtonBuilder buttonBuilder = new ButtonBuilder();
            buttonBuilder.WithLabel(label);
            buttonBuilder.WithCustomId(id);
            buttonBuilder.WithStyle(style);
            return buttonBuilder;
        }
    }
}
