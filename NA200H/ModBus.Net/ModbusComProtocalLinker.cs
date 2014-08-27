using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    class ModbusComProtocalLinker : ComProtocalLinker
    {
        public override bool CheckRight(byte[] content)
        {
            if (!Crc16.GetInstance().CrcEfficacy(content))
            {
                throw new ModbusProtocalErrorException(501);
            }
            if (content[1] > 127)
            {
                throw new ModbusProtocalErrorException(content[2]);
            }
            return true;
        }

        public ModbusComProtocalLinker() : base()
        {
            
        }

        public ModbusComProtocalLinker(string com) : base(com)
        {
            
        }
    }
}
