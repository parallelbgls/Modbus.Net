using System.Configuration;
using System.IO.Ports;

namespace Modbus.Net
{
    /// <summary>
    ///     串口连接对象
    /// </summary>
    public abstract class ComProtocalLinker : ProtocalLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="slaveAddress">从站地址</param>
        protected ComProtocalLinker(int baudRate, Parity parity, StopBits stopBits, int dataBits, int slaveAddress)
            : this(ConfigurationManager.AppSettings["COM"], baudRate, parity, stopBits, dataBits, slaveAddress)
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
        protected ComProtocalLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits,
            int slaveAddress)
            : this(
                com, baudRate, parity, stopBits, dataBits,
                int.Parse(ConfigurationManager.AppSettings["ComConnectionTimeout"] ?? "3000"), slaveAddress)
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
        protected ComProtocalLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits,
            int connectionTimeout, int slaveAddress)
        {
            BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate, parity, stopBits, dataBits,
                connectionTimeout);
        }
    }
}