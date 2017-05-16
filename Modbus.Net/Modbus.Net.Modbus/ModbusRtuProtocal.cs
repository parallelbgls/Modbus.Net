using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress, endian)
        {
        }

        public ModbusRtuProtocal(string com, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocalLinker = new ModbusRtuProtocalLinker(com, slaveAddress);
        }
    }
}