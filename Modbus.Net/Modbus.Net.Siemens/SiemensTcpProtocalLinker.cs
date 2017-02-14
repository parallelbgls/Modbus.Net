using System;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Tcp协议连接器
    /// </summary>
    public class SiemensTcpProtocalLinker : TcpProtocalLinker
    {
        public SiemensTcpProtocalLinker(string ip)
            : base(ip, int.Parse(ConfigurationManager.SiemensPort))
        {
        }

        public SiemensTcpProtocalLinker(string ip, int port)
            : base(ip, port)
        {
        }

        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            switch (content[5])
            {
                case 0xd0:
                case 0xe0:
                    return true;
                case 0xf0:
                    switch (content[8])
                    {
                        case 0x03:
                            if (content[17] == 0x00 && content[18] == 0x00) return true;
                            throw new SiemensProtocalErrorException(content[17], content[18]);
                        case 0x07:
                            if (content[27] == 0x00 && content[28] == 0x00) return true;
                            throw new SiemensProtocalErrorException(content[27], content[28]);
                    }
                    return true;
                default:
                    throw new FormatException();
            }
        }
    }
}