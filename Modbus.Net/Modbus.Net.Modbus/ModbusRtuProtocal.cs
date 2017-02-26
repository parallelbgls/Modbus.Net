namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal(byte slaveAddress, byte masterAddress, Endian endian)
            : this(ConfigurationManager.COM, slaveAddress, masterAddress, endian)
        {
        }

        public ModbusRtuProtocal(string com, byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
            ProtocalLinker = new ModbusRtuProtocalLinker(com);
        }
    }
}