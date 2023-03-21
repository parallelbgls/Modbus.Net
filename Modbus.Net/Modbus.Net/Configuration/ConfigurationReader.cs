using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Modbus.Net
{
    /// <summary>
    ///     Modbus.Net专用配置读取类
    /// </summary>
    public static class ConfigurationReader
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.default.json")
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

#nullable enable
        /// <summary>
        ///     根据路径，依次查找路径与父路径上是否有该元素
        /// </summary>
        /// <param name="path">路径，冒号隔开</param>
        /// <param name="key">元素的键</param>
        /// <returns>元素的值</returns>
        public static string? GetValue(string path, string key)
        {
            var split = path.Split(":");
            string? ans = null;
            while (split.Length > 0)
            {
                var root = configuration.GetSection("Modbus.Net");
                foreach (var entry in split)
                {
                    root = root?.GetSection(entry);
                }
                ans = ans ?? root?[key];
                split = split.Take(split.Length - 1).ToArray();
            }
            return ans;
        }

        /// <summary>
        ///     根据路径，直接查找路径上是否有该元素
        /// </summary>
        /// <param name="path">路径，冒号隔开</param>
        /// <param name="key">元素的键</param>
        /// <returns>元素的值</returns>
        public static string? GetValueDirect(string path, string key)
        {
            var root = configuration.GetSection("Modbus.Net");
            var firstColon = path.IndexOf(":");
            while (firstColon != -1)
            {
                root = root?.GetSection(path.Substring(0, firstColon));
                path = path.Substring(firstColon + 1);
                firstColon = path.IndexOf(":");
            }
            root = root?.GetSection(path);
            return root?[key];
        }
#nullable disable
    }
}
