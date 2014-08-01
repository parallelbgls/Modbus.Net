using System;

namespace ModBus.Net
{
    public class ModbusTcpProtocal : ModbusProtocal
    {
        public ModbusTcpProtocal()
        {
            _protocalLinker = new TcpProtocalLinker();
        }
    }
}