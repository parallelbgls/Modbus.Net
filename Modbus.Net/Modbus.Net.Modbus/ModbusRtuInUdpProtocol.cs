using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议udp透传
    /// </summary>
    public class ModbusRtuInUdpProtocol : ModbusProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ModbusRtuInUdpProtocol(byte slaveAddress, byte masterAddress)
            : this(ConfigurationReader.GetValueDirect("UDP:Modbus", "IP"), slaveAddress, masterAddress)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ModbusRtuInUdpProtocol(string ip, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocolLinker = new ModbusRtuInUdpProtocolLinker(ip);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public ModbusRtuInUdpProtocol(string ip, int port, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocolLinker = new ModbusRtuInUdpProtocolLinker(ip, port);
        }
    }
}
