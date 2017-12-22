using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议
    /// </summary>
    public class ModbusAsciiProtocol : ModbusProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusAsciiProtocol(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusAsciiProtocol(string com, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusAsciiProtocolLinker(com, slaveAddress);
        }
    }
}