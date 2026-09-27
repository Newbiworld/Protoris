using Microsoft.Extensions.Configuration;

namespace Protoris.Service.Config
{
    public class SpotifyConfig : ISpotifyConfig
    {
        public SpotifyConfig(IConfiguration config)
        {
            if (config != null)
            {
                SpotifyClientId = config.GetValue<string>("SpotifyClientId");
                SpotifyClientSecret = config.GetValue<string>("SpotifyClientSecret");
            }
        }

        public string SpotifyClientId { get; private set; }
        public string SpotifyClientSecret { get; private set; }

    }
}
