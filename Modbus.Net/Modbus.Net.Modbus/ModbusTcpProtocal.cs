namespace Modbus.Net.Modbus
{
    /// <summary>
    /// Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal(byte belongAddress, byte masterAddress) : this(ConfigurationManager.IP, belongAddress, masterAddress)
        {
        }

        public ModbusTcpProtocal(string ip, byte belongAddress, byte masterAddress) : base(belongAddress, masterAddress)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip);
        }

        public ModbusTcpProtocal(string ip, int port, byte belongAddress, byte masterAddress) : base(belongAddress, masterAddress)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip, port);
        }
    }
}