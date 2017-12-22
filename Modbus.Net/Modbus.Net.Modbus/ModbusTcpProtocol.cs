using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocol : ModbusProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusTcpProtocol(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["IP"], slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusTcpProtocol(string ip, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusTcpProtocolLinker(ip);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusTcpProtocol(string ip, int port, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusTcpProtocolLinker(ip, port);
        }
    }
}