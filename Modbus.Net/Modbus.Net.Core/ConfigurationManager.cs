using System.IO;
using Microsoft.Extensions.Configuration;

namespace Modbus.Net
{
    /// <summary>
    ///     Simulate ConfigurationManager in System.Configuration
    /// </summary>
    public static class ConfigurationManager
    {
        /// <summary>
        ///     Configuration Builder
        /// </summary>
        private static readonly IConfigurationBuilder Builder = new ConfigurationBuilder()
            .SetBasePath(RootPath ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddXmlFile("App.config");

        /// <summary>
        ///     RootPath of App.config and appsettings.json
        /// </summary>
        public static string RootPath { get; set; } = null;

        /// <summary>
        ///     Configuration Root
        /// </summary>
        private static IConfigurationRoot Configuration => Builder.Build();

        /// <summary>
        ///     AppSettings
        /// </summary>
        public static IConfigurationSection AppSettings => Configuration.GetSection("AppSettings");

        /// <summary>
        ///     ConnectionStrings
        /// </summary>
        public static IConfigurationSection ConnectionStrings => Configuration.GetSection("ConnectionStrings");
    }
}