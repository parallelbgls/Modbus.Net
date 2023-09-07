﻿namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器Udp透传
    /// </summary>
    public class ModbusRtuInUdpProtocolLinker : UdpProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusRtuInUdpProtocolLinker(string ip)
            : base(ip, int.Parse(ConfigurationReader.GetValueDirect("UDP:" + ip, "ModbusPort") ?? ConfigurationReader.GetValueDirect("UDP:Modbus", "ModbusPort")))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public ModbusRtuInUdpProtocolLinker(string ip, int port)
            : base(ip, port)
        {
        }

        /// <summary>
        ///     校验返回数据
        /// </summary>
        /// <param name="content">设备返回的数据</param>
        /// <returns>数据是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            if (base.CheckRight(content) != true) return base.CheckRight(content);
            //Modbus协议错误
            if (content[1] > 127)
                throw new ModbusProtocolErrorException(content[2]);
            return true;
        }
    }
}
