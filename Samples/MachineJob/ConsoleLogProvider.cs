using Quartz.Logging;

namespace MachineJob
{
    // simple log provider to get something to the console
    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
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
