using Protoris.Service.Config;
using Protoris.Service.Interfaces;

namespace Protoris.Service
{
    public class ExceptionService : IExceptionService
    {
        private readonly IFileConfig _fileConfig;
        public ExceptionService(IFileConfig fileConfig)
        {
            _fileConfig = fileConfig;
        }

        public void LogException(Exception exception)
        {
            string path = _fileConfig.ErrorLogsFilePath;
            Directory.CreateDirectory(path);
            DateTime timeOfException = DateTime.Now;
            string filePath = $"{path}\\{timeOfException.ToString("yyyy-MM-dd_HHmmss")}.txt";

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("An exception occured! Deal with this (sometime) \n");
                sw.WriteLine(exception.Message);
                sw.WriteLine(exception.StackTrace);
            }
        }
    }
}
