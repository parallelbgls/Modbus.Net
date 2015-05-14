using System;

namespace ModBus.Net
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
    }
}