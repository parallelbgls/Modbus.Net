using System.Text;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议连接器Udp透传
    /// </summary>
    public class ModbusAsciiInUdpProtocolLinker : UdpProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusAsciiInUdpProtocolLinker(string ip)
            : base(ip, int.Parse(ConfigurationReader.GetValueDirect("UDP:" + ip, "ModbusPort") ?? ConfigurationReader.GetValueDirect("UDP:Modbus", "ModbusPort")))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusAsciiInUdpProtocolLinker(string ip, int port)
            : base(ip, port)
        {
        }

        /// <summary>
        ///     校验返回数据是否正确
        /// </summary>
        /// <param name="content">返回的数据</param>
        /// <returns>校验是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocolLinker不会返回null
            if (base.CheckRight(content) != true) return false;
            //Modbus协议错误
            var contentString = Encoding.ASCII.GetString(content);
            if (byte.Parse(contentString.Substring(3, 2)) > 127)
                throw new ModbusProtocolErrorException(byte.Parse(contentString.Substring(5, 2)));
            return true;
        }
    }
}
