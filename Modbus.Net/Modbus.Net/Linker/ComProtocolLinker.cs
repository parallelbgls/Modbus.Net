using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.IO.Ports;

namespace Modbus.Net
{
    /// <summary>
    ///     串口连接对象
    /// </summary>
    public abstract class ComProtocolLinker : ProtocolLinker
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        protected ComProtocolLinker(int slaveAddress)
            : this(configuration.GetSection("Modbus.Net")["COM"] ?? "COM1", slaveAddress)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="com">串口端口号</param>
        /// <param name="slaveAddress">从站地址</param>
        protected ComProtocolLinker(string com, int slaveAddress)
            : this(com, slaveAddress, int.Parse(configuration.GetSection("Modbus.Net")["ComBaudRate"] ?? "9600"), Enum.Parse<Parity>(configuration.GetSection("Modbus.Net")["ComParity"] ?? "Parity.None"), Enum.Parse<StopBits>(configuration.GetSection("Modbus.Net")["ComStopBits"] ?? "StopBits.One"), int.Parse(configuration.GetSection("Modbus.Net")["ComDataBits"] ?? "8"))
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="com">串口端口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected ComProtocolLinker(string com, int slaveAddress, int baudRate, Parity parity, StopBits stopBits, int dataBits,
             bool isFullDuplex = false)
            : this(
                com, slaveAddress, baudRate, parity, stopBits, dataBits,
                int.Parse(configuration.GetSection("Modbus.Net")["ComConnectionTimeout"] ?? "-1"), isFullDuplex)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="com">串口端口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="connectionTimeout">超时时间</param>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected ComProtocolLinker(string com, int slaveAddress, int baudRate, Parity parity, StopBits stopBits, int dataBits,
            int connectionTimeout,  bool isFullDuplex = false)
        {
            if (connectionTimeout == -1)
            {
                BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate, parity, stopBits, dataBits, isFullDuplex: isFullDuplex);
            }
            else
            {
                BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate, parity, stopBits, dataBits,
                    connectionTimeout, isFullDuplex: isFullDuplex);
            }

        }
    }
}