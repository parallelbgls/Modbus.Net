using System.Collections.Generic;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器Tcp透传
    /// </summary>
    public class ModbusRtuInTcpProtocolLinker : TcpProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusRtuInTcpProtocolLinker(string ip)
            : base(ip, int.Parse(ConfigurationReader.GetValueDirect("TCP:" + ip, "ModbusPort") ?? ConfigurationReader.GetValueDirect("TCP:Modbus", "ModbusPort")))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public ModbusRtuInTcpProtocolLinker(string ip, int port)
            : base(ip, port)
        {
            ((EventHandlerConnector)BaseConnector).AddController(new FifoController(int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "FetchSleepTime")), lengthCalc: content => { if (content[1] == 5 || content[1] == 6 || content[1] == 15 || content[1] == 16 || content[1] == 21) return 8; else return DuplicateWithCount.GetDuplcateFunc(new List<int> { 2 }, 5).Invoke(content); }, waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null ? int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount")) : null));
        }

        /// <summary>
        ///     校验返回数据
        /// </summary>
        /// <param name="content">设备返回的数据</param>
        /// <returns>数据是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocolLinker的CheckRight不会返回null
            if (base.CheckRight(content) != true) return false;
            //CRC校验失败
            if (!Crc16.GetInstance().CrcEfficacy(content))
                throw new ModbusProtocolErrorException(501);
            //Modbus协议错误
            if (content[1] > 127)
                throw new ModbusProtocolErrorException(content[2]);
            return true;
        }
    }
}