using Microsoft.Extensions.Configuration;

namespace Protoris.Service.Config
{
    public class FileConfig : IFileConfig
    {
        public FileConfig(IConfiguration config)
        {
            if (config != null)
            {
                ErrorLogsFilePath = $"{config.GetValue<string>("FilePath")}\\error";
            }
        }

        public string ErrorLogsFilePath { get; private set; }
    }
}
