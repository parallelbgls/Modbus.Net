using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["IP"], slaveAddress, masterAddress, endian)
        {
        }

        public ModbusTcpProtocal(string ip, byte slaveAddress, byte masterAddress, Endian endian) : base(slaveAddress, masterAddress, endian)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip);
        }

        public ModbusTcpProtocal(string ip, int port, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip, port);
        }
    }
}