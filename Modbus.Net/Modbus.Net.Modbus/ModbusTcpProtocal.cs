namespace Modbus.Net.Modbus
{
    /// <summary>
    /// Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal() : this(ConfigurationManager.IP)
        {
        }

        public ModbusTcpProtocal(string ip)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip);
        }

        public ModbusTcpProtocal(string ip, int port)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip, port);
        }
    }
}