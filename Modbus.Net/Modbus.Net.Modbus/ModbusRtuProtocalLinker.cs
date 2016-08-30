namespace Modbus.Net.Modbus
{
    class ModbusRtuProtocalLinker : ComProtocalLinker
    {
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

        public ModbusRtuProtocalLinker(string com) : base(com)
        {
            
        }
    }
}
