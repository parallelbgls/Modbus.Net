namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal(byte slaveAddress, byte masterAddress)
            : this(ConfigurationManager.COM, slaveAddress, masterAddress)
        {
        }

        public ModbusRtuProtocal(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocalLinker = new ModbusRtuProtocalLinker(com);
        }
    }
}