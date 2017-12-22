using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocol : ModbusProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusRtuProtocol(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusRtuProtocol(string com, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocolLinker = new ModbusRtuProtocolLinker(com, slaveAddress);
        }
    }
}