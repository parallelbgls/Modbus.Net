using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议
    /// </summary>
    public class ModbusAsciiProtocal : ModbusProtocal
    {
        public ModbusAsciiProtocal(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress, endian)
        {
        }

        public ModbusAsciiProtocal(string com, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocalLinker = new ModbusAsciiProtocalLinker(com, slaveAddress);
        }
    }
}