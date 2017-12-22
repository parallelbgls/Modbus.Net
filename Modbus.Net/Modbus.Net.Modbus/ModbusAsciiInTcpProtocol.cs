using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议
    /// </summary>
    public class ModbusAsciiInTcpProtocol : ModbusProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusAsciiInTcpProtocol(byte slaveAddress, byte masterAddress, Endian endian)
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
        public ModbusAsciiInTcpProtocol(string ip, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusAsciiInTcpProtocolLinker(ip, slaveAddress);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusAsciiInTcpProtocol(string ip, int port, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusTcpProtocolLinker(ip, port);
        }
    }
}