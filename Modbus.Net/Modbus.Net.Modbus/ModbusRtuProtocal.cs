namespace Modbus.Net.Modbus
{
    /// <summary>
    /// Modbus/Rtu协议
    /// </summary>
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal() : this(ConfigurationManager.COM)
        {
        }

        public ModbusRtuProtocal(string com)
        {
            ProtocalLinker = new ModbusRtuProtocalLinker(com);
        }
    }
}
