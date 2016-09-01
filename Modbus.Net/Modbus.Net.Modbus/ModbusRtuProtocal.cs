namespace Modbus.Net.Modbus
{
    /// <summary>
    /// Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal(byte belongAddress, byte masterAddress) : this(ConfigurationManager.COM, belongAddress, masterAddress)
        {
        }

        public ModbusRtuProtocal(string com, byte belongAddress, byte masterAddress) : base(belongAddress, masterAddress)
        {
            ProtocalLinker = new ModbusRtuProtocalLinker(com);
        }
    }
}
