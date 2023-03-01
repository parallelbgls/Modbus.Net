using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Modbus.Net
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogProvider
    {
        private static ILoggerFactory _loggerFactory = null;

        /// <summary>
        /// Sets the current log provider based on logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void SetLogProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static ILogger CreateLogger(string category) => _loggerFactory != null ? _loggerFactory.CreateLogger(category) : NullLogger.Instance;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>() => _loggerFactory != null ? _loggerFactory.CreateLogger<T>() : NullLogger<T>.Instance;

#if DIAGNOSTICS_SOURCE
        internal static class Cached
        {
            internal static readonly System.Lazy<System.Diagnostics.DiagnosticListener> Default =
                new(() => new System.Diagnostics.DiagnosticListener(DiagnosticHeaders.DefaultListenerName));
        }
#endif
    }
}