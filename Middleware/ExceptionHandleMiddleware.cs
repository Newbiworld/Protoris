
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Protoris.Service.Config;

namespace Protoris.Middleware
{
    public class ExceptionHandleMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IFileConfig _fileConfig;
        public ExceptionHandleMiddleware(IFileConfig fileConfig)
        {
            _fileConfig = fileConfig;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                string path = _fileConfig.ErrorLogsFilePath;
                Directory.CreateDirectory(path);
                DateTime timeOfException = DateTime.Now;
                string filePath = $"{path}\\{timeOfException.ToString("yyyy-MM-dd_HHmmss")}.txt";

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("An exception occured! Deal with this (sometime) \n");
                    sw.WriteLine(e.Message);
                    sw.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
