using System.IO.Ports;

namespace Modbus.Net
{
    /// <summary>
    ///     串口连接对象
    /// </summary>
    public abstract class ComProtocalLinker : ProtocalLinker
    {
        protected ComProtocalLinker(int baudRate, Parity parity, StopBits stopBits, int dataBits)
            : this(ConfigurationManager.COM, baudRate, parity, stopBits, dataBits)
        {
        }

        protected ComProtocalLinker(string com, int baudRate, Parity parity, StopBits stopBits, int dataBits)
        {
            //初始化连对象
            BaseConnector = new ComConnector(com, baudRate, parity, stopBits, dataBits, 30000);
        }
    }
}