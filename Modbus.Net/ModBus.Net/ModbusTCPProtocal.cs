using System;

namespace ModBus.Net
{
    /// <summary>
    /// Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal()
        {
            _protocalLinker = new ModbusTcpProtocalLinker();
        }

        public ModbusTcpProtocal(string ip)
        {
            _protocalLinker = new ModbusTcpProtocalLinker(ip);
        }
    }
}