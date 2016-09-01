using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    /// <summary>
    /// Modbus/Rtu协议
    /// </summary>
    public class ModbusAsciiProtocal : ModbusProtocal
    {
        public ModbusAsciiProtocal() : this(ConfigurationManager.COM)
        {
        }

        public ModbusAsciiProtocal(string com)
        {
            ProtocalLinker = new ModbusAsciiProtocalLinker(com);
        }
    }
}
