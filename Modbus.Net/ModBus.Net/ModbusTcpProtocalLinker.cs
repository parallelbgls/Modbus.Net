using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public class ModbusTcpProtocalLinker : TcpProtocalLinker
    {
        public override bool CheckRight(byte[] content)
        {
            if (content[5] != content.Length - 6)
            {
                throw new ModbusProtocalErrorException(500);
            }
            if (content[7] > 127)
            {
                throw new ModbusProtocalErrorException(content[2]);
            }
            return true;
        }

        public ModbusTcpProtocalLinker() : base()
        {
            
        }

        public ModbusTcpProtocalLinker(string ip) : base(ip)
        {
            
        }
    }
}
