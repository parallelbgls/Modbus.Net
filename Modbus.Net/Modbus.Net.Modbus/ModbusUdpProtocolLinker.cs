using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Udp协议连接器
    /// </summary>
    public class ModbusUdpProtocolLinker : UdpProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusUdpProtocolLinker(string ip)
            : this(ip, int.Parse(ConfigurationReader.GetValue("UDP:" + ip, "ModbusPort")))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public ModbusUdpProtocolLinker(string ip, int port) : base(ip, port)
        {
            ((BaseConnector)BaseConnector).AddController(new FifoController(int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "FetchSleepTime")), waitingListMaxCount: ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount") != null ? int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount")) : null));
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
            //长度校验失败
            if (content[5] != content.Length - 6)
                throw new ModbusProtocolErrorException(500);
            //Modbus协议错误
            if (content[7] > 127)
                throw new ModbusProtocolErrorException(content[2] > 0 ? content[2] : content[8]);
            return true;
        }
    }
}