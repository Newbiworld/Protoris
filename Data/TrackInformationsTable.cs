using Discord;
using Protoris.Service;
using Victoria;

namespace Protoris.Data
{
    public class TrackInformationsTable
    {
        private readonly List<TrackInformations> _tracks;
        private readonly int _pageSize = 10;
        private int _currentPage;
        private string _typeId;
        private string _actionTypeId;

        public TrackInformationsTable(List<TrackInformations> tracks,
            int currentPage,
            string typeId,
            string actionTypeId)
        {
            _tracks = tracks;
            _currentPage = currentPage;
            _typeId = typeId;
            _actionTypeId = actionTypeId;
        }
        private bool HasNextPage => _tracks.Count > (_currentPage * _pageSize) + _pageSize;

        public void ApplyTableToContainer(ContainerBuilder playlistContainer, Func<string, ButtonBuilder> addButtonFunction)
        {
            if (!_tracks.Any())
            {
                playlistContainer.WithTextDisplay($"But he had nothing to show");
            }
            else
            {
                if (_tracks.Count <= _currentPage * _pageSize)
                {
                    _currentPage = 0;
                }

                for (int i = 0; i < _pageSize; i++)
                {
                    int currentIndex = _currentPage * _pageSize + i;
                    if (currentIndex >= _tracks.Count) break;

                    TrackInformations trackInfo = _tracks[currentIndex];
                    string id = trackInfo.Id;

                    AddTrackToTable(trackInfo,
                        playlistContainer,
                        addButtonFunction,
                        $"{_actionTypeId}{id}",
                        currentIndex + 1);
                }

                AddFooterToTable(playlistContainer);
            }
        }

        private void AddTrackToTable(TrackInformations trackInfo,
            ContainerBuilder playlistContainer,
            Func<string, ButtonBuilder> addButtonFunction,
            string buttonId,
            int number)
        {
            LavaTrack track = trackInfo.Track;
            string id = trackInfo.Id;

            SectionBuilder trackSection = new SectionBuilder();
            trackSection.WithAccessory(addButtonFunction(buttonId));
            trackSection.WithTextDisplay($"**{number}.** [{track.Title}]({track.Url}) | Duration: {track.Duration.ToString(@"mm\:ss")}");
            playlistContainer.AddComponent(trackSection);
        }

        private void AddFooterToTable(ContainerBuilder playlistContainer)
        {
            playlistContainer.WithTextDisplay($"**Current Page: {_currentPage + 1}**");

            if (HasNextPage || _currentPage > 0)
            {
                List<ButtonBuilder> buttons = new List<ButtonBuilder>();

                if (_currentPage > 0)
                {
                    buttons.Add(DiscordComponentHelper.CreateButton("Previous", $"{_typeId}{_currentPage - 1}", ButtonStyle.Secondary));
                }

                if (HasNextPage)
                {
                    buttons.Add(DiscordComponentHelper.CreateButton("Next", $"{_typeId}{_currentPage + 1}", ButtonStyle.Secondary));
                }

                playlistContainer.WithActionRow(buttons);
            }
        }
    }
}
