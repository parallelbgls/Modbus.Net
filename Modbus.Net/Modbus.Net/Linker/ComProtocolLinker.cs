using Microsoft.Extensions.Configuration;
using System.IO.Ports;

namespace Modbus.Net
{
    /// <summary>
    ///     串口连接对象
    /// </summary>
    public abstract class ComProtocolLinker : ProtocolLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="slaveAddress">从站地址</param>
        protected ComProtocolLinker(int baudRate, Parity parity, StopBits stopBits, int dataBits, int slaveAddress)
            : this(new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build().GetSection("Config")["COM"], baudRate, parity, stopBits, dataBits, slaveAddress)
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
        protected ComProtocolLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits,
            int slaveAddress, bool isFullDuplex = false)
            : this(
                com, baudRate, parity, stopBits, dataBits,
                int.Parse(new ConfigurationBuilder().AddJsonFile($"appsettings.json").Build().GetSection("Config")["ComConnectionTimeout"] ?? "-1"), slaveAddress, isFullDuplex)
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
        protected ComProtocolLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits,
            int connectionTimeout, int slaveAddress, bool isFullDuplex = false)
        {
            if (connectionTimeout == -1)
            {
                BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate, parity, stopBits, dataBits, isFullDuplex:isFullDuplex);
            }
            else
            {
                BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate, parity, stopBits, dataBits,
                    connectionTimeout, isFullDuplex:isFullDuplex);
            }
            
        }
    }
}