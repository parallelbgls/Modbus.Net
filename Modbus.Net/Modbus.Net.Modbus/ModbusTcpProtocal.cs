namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal(byte slaveAddress, byte masterAddress)
            : this(ConfigurationManager.IP, slaveAddress, masterAddress)
        {
        }

        public ModbusTcpProtocal(string ip, byte slaveAddress, byte masterAddress) : base(slaveAddress, masterAddress)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip);
        }

        public ModbusTcpProtocal(string ip, int port, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            ProtocalLinker = new ModbusTcpProtocalLinker(ip, port);
        }
    }
}