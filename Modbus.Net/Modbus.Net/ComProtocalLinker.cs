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
        protected ComProtocalLinker(int baudRate, Parity parity, StopBits stopBits, int dataBits)
            : this(ConfigurationManager.COM, baudRate, parity, stopBits, dataBits)
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
        protected ComProtocalLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits)
        {
            //初始化连对象
            BaseConnector = new ComConnector(com, baudRate, parity, stopBits, dataBits, 30000);
        }
    }
}