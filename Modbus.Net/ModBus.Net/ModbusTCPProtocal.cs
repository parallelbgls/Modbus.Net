using System;

namespace ModBus.Net
{
    /// <summary>
    /// Modbus/Tcp协议
    /// </summary>
    public class ModbusTcpProtocal : ModbusProtocal
    {
        //将连接器设置为Tcp连接器
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