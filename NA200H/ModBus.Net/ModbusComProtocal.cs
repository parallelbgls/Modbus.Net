using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public class ModbusComProtocal : ModbusProtocal
    {
        public ModbusComProtocal()
        {
            _protocalLinker = new ModbusComProtocalLinker();
        }

        public ModbusComProtocal(string com)
        {
            _protocalLinker = new ModbusComProtocalLinker(com);
        }
    }
}
