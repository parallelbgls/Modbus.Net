using System.IO.Ports;

namespace Modbus.Net.Modbus
{
    public class ModbusRtuProtocalLinker : ComProtocalLinker
    {
        public ModbusRtuProtocalLinker(string com) : base(com, 9600, Parity.None, StopBits.One, 8)
        {
        }

        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
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
    }
}