using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Modbus.Net
{
    public static class ConfigurationManager
    {
        private static IConfigurationBuilder builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddXmlFile("App.config");

        private static IConfigurationRoot Configuration => builder.Build();

        public static IConfigurationSection AppSettings => Configuration.GetSection("AppSettings");
    }
}