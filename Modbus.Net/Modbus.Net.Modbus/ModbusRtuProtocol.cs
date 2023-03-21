using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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
        public ModbusRtuProtocol(byte slaveAddress, byte masterAddress)
            : this(ConfigurationReader.GetValueDirect("COM:Modbus", "COM"), slaveAddress, masterAddress)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ModbusRtuProtocol(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocolLinker = new ModbusRtuProtocolLinker(com, slaveAddress);
        }
    }
}