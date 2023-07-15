using Quartz.Logging;
using System.Configuration;

namespace MachineJob
{
    // simple log provider to get something to the console
    public class ConsoleLogProvider : ILogProvider
    {
        private readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= configuration.GetSection("Quartz").GetValue<Quartz.Logging.LogLevel>("LogLevel") && func != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }
}
