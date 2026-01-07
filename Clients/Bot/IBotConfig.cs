namespace Protoris.Clients.Bot
{
    public interface IBotConfig
    {
        public string TokenKey { get; }
        public string TestingServerId { get; }
        public bool IsBetaTesting { get; }
    }
}
