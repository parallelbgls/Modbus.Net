namespace Modbus.Net.Modbus
{
    public class ModbusTcpProtocalLinker : TcpProtocalLinker
    {
        public ModbusTcpProtocalLinker(string ip) : base(ip, int.Parse(ConfigurationManager.ModbusPort))
        {
        }

        public ModbusTcpProtocalLinker(string ip, int port) : base(ip, port)
        {
        }

        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            //长度校验失败
            if (content[5] != content.Length - 6)
            {
                throw new ModbusProtocalErrorException(500);
            }
            //Modbus协议错误
            if (content[7] > 127)
            {
                throw new ModbusProtocalErrorException(content[8]);
            }
            return true;
        }
    }
}