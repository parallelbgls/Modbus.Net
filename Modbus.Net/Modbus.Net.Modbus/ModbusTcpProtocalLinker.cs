namespace ModBus.Net.Modbus
{
    public class ModbusTcpProtocalLinker : TcpProtocalLinker
    {
        public override bool CheckRight(byte[] content)
        {
            if (!base.CheckRight(content)) return false;
            //长度校验失败
            if (content[5] != content.Length - 6)
            {
                throw new ModbusProtocalErrorException(500);
            }
            //Modbus协议错误
            if (content[7] > 127)
            {
                throw new ModbusProtocalErrorException(content[2]);
            }
            return true;
        }

        public ModbusTcpProtocalLinker(string ip) : base(ip, int.Parse(ConfigurationManager.ModbusPort))
        {
            
        }
    }
}
