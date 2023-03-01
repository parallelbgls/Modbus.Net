using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议连接器Udp透传
    /// </summary>
    public class ModbusAsciiInUdpProtocolLinker : UdpProtocolLinker
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusAsciiInUdpProtocolLinker(string ip)
            : base(ip, int.Parse(configuration.GetSection("Modbus.Net")["ModbusPort"] ?? "502"), false)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusAsciiInUdpProtocolLinker(string ip, int port)
            : base(ip, port)
        {
            ((BaseConnector)BaseConnector).AddController(new FifoController(int.Parse(configuration.GetSection("Modbus.Net")["FetchSleepTime"] ?? "0")));
        }

        /// <summary>
        ///     校验返回数据是否正确
        /// </summary>
        /// <param name="content">返回的数据</param>
        /// <returns>校验是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocolLinker不会返回null
            if (base.CheckRight(content) != true) return false;
            //CRC校验失败
            var contentString = Encoding.ASCII.GetString(content);
            if (!Crc16.GetInstance().LrcEfficacy(contentString))
                throw new ModbusProtocolErrorException(501);
            //Modbus协议错误
            if (byte.Parse(contentString.Substring(3, 2)) > 127)
                throw new ModbusProtocolErrorException(byte.Parse(contentString.Substring(5, 2)));
            return true;
        }
    }
}
