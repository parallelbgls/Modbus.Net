namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议
    /// </summary>
    public class ModbusAsciiProtocal : ModbusProtocal
    {
        public ModbusAsciiProtocal(byte slaveAddress, byte masterAddress)
            : this(ConfigurationManager.COM, slaveAddress, masterAddress)
        {
        }

        public ModbusAsciiProtocal(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocalLinker = new ModbusAsciiProtocalLinker(com);
        }
    }
}