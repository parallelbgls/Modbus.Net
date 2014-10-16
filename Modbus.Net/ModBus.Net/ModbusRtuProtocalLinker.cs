using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    class ModbusRtuProtocalLinker : ComProtocalLinker
    {
        public override bool CheckRight(byte[] content)
        {
            //CRC校验失败
            if (!Crc16.GetInstance().CrcEfficacy(content))
            {
                throw new ModbusProtocalErrorException(501);
            }
            //Modbus协议错误
            if (content[1] > 127)
            {
                throw new ModbusProtocalErrorException(content[2]);
            }
            return true;
        }

        public ModbusRtuProtocalLinker(string com) : base(com)
        {
            
        }
    }
}
