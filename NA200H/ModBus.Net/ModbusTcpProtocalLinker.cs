using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    class ModbusTcpProtocalLinker : TcpProtocalLinker
    {
        public override bool CheckRight(byte[] content)
        {
            if (content[1] > 127)
            {
                throw new ModbusProtocalErrorException(content[2]);
            }
            return true;
        }
    }
}
