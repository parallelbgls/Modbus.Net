using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Modbus.Net
{
    /// <summary>
    ///     Udp连接对象
    /// </summary>
    public abstract class UdpProtocolLinker : ProtocolLinker
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        /// <summary>
        ///     构造器
        /// </summary>
        protected UdpProtocolLinker(int port)
            : this(configuration.GetSection("Modbus.Net")["IP"], port)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected UdpProtocolLinker(string ip, int port, bool isFullDuplex = true)
            : this(ip, port, int.Parse(configuration.GetSection("Modbus.Net")["IPConnectionTimeout"] ?? "-1"), isFullDuplex)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="connectionTimeout">超时时间</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected UdpProtocolLinker(string ip, int port, int connectionTimeout, bool isFullDuplex = true)
        {
            if (connectionTimeout == -1)
            {
                //初始化连接对象
                BaseConnector = new UdpConnector(ip, port, isFullDuplex: isFullDuplex);
            }
            else
            {
                //初始化连接对象
                BaseConnector = new UdpConnector(ip, port, connectionTimeout, isFullDuplex: isFullDuplex);
            }
        }
    }
}