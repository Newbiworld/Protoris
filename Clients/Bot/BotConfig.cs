using Microsoft.Extensions.Configuration;

namespace Protoris.Clients.Bot
{
    public class BotConfig : IBotConfig
    {
        public BotConfig(IConfiguration config)
        {
            if (config != null)
            {
                TokenKey = config.GetValue<string>("BotToken");
                TestingServerId = config.GetValue<string>("BotTestingGround");
                IsBetaTesting = config.GetValue<bool>("IsBetaTesting");
            }
        }

        public string TokenKey { get; private set; }
        public string TestingServerId { get; private set; }
        public bool IsBetaTesting { get; private set; }
    }
}
