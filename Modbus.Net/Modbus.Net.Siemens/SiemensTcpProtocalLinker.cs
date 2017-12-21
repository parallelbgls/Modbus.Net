using System;
using System.Configuration;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Tcp协议连接器
    /// </summary>
    public class SiemensTcpProtocalLinker : TcpProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public SiemensTcpProtocalLinker(string ip)
            : this(ip, int.Parse(ConfigurationManager.AppSettings["SiemensPort"] ?? "102"))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public SiemensTcpProtocalLinker(string ip, int port)
            : base(ip, port)
        {
            ((BaseConnector)BaseConnector).AddController(new FIFOController(500));
        }

        /// <summary>
        ///     校验报文
        /// </summary>
        /// <param name="content">设备返回的信息</param>
        /// <returns>报文是否正确</returns>
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
                        case 0x01:
                        case 0x02:
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