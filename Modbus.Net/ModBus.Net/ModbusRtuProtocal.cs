using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public class ModbusRtuProtocal : ModbusProtocal
    {
        public ModbusRtuProtocal()
        {
            _protocalLinker = new ModbusRtuProtocalLinker();
        }

        public ModbusRtuProtocal(string com)
        {
            _protocalLinker = new ModbusRtuProtocalLinker(com);
        }
    }
}
