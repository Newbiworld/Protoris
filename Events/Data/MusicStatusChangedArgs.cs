
using Protoris.Enum;

namespace Protoris.Events.Data
{
    public class MusicStatusChangedArgs : EventArgs
    {
        public ulong GuildId { get; set; }
        public EMusicStatus MusicStatus { get; set; }
    }
}
