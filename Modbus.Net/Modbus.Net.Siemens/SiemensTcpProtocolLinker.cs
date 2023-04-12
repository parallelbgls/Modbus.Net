using System;
using System.Collections.Generic;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Tcp协议连接器
    /// </summary>
    public class SiemensTcpProtocolLinker : TcpProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public SiemensTcpProtocolLinker(string ip)
            : this(ip, int.Parse(ConfigurationReader.GetValueDirect("TCP:" + ip, "Siemens") ?? ConfigurationReader.GetValueDirect("TCP:Siemens", "SiemensPort")))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public SiemensTcpProtocolLinker(string ip, int port)
            : base(ip, port)
        {
            ((IConnectorWithController<byte[], byte[]>)BaseConnector).AddController(new MatchDirectlySendController(
                new ICollection<(int, int)>[] { new List<(int, int)> { (11, 11), (12, 12) } },
                lengthCalc: DuplicateWithCount.GetDuplcateFunc(new List<int> { 2, 3 }, 0),
                waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount")) :
                  null
             ));
        }

        /// <summary>
        ///     校验报文
        /// </summary>
        /// <param name="content">设备返回的信息</param>
        /// <returns>报文是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            if (base.CheckRight(content) != true) return false;
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
                            throw new SiemensProtocolErrorException(content[17], content[18]);
                        case 0x07:
                            if (content[27] == 0x00 && content[28] == 0x00) return true;
                            throw new SiemensProtocolErrorException(content[27], content[28]);
                    }
                    return true;
                default:
                    throw new FormatException($"Error content code with code {content[5]} {content[8]}");
            }
        }
    }
}