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
        public ModbusAsciiProtocal(byte belongAddress, byte masterAddress) : this(ConfigurationManager.COM, belongAddress, masterAddress)
        {
        }

        public ModbusAsciiProtocal(string com, byte belongAddress, byte masterAddress) : base(belongAddress, masterAddress)
        {
            ProtocalLinker = new ModbusAsciiProtocalLinker(com);
        }
    }
}
