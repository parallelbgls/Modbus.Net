using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Modbus
{
    public class ModbusAsciiProtocalLinker : ComProtocalLinker
    {
        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            //CRC校验失败
            string contentString = Encoding.ASCII.GetString(content);
            if (!Crc16.GetInstance().LrcEfficacy(contentString))
            {
                throw new ModbusProtocalErrorException(501);
            }
            //Modbus协议错误
            if (byte.Parse(contentString.Substring(3, 2)) > 127)
            {
                throw new ModbusProtocalErrorException(byte.Parse(contentString.Substring(5, 2)));
            }
            return true;
        }

        public ModbusAsciiProtocalLinker(string com) : base(com, 9600, Parity.None, StopBits.One, 8)
        {
        }
    }
}
