using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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
        public ModbusAsciiProtocol(byte slaveAddress, byte masterAddress)
            : this(ConfigurationReader.GetValueDirect("COM:Modbus", "COM"), slaveAddress, masterAddress)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ModbusAsciiProtocol(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocolLinker = new ModbusAsciiProtocolLinker(com, slaveAddress);
        }
    }
}