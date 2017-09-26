using System.Configuration;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器
    /// </summary>
    public class ModbusRtuInTcpProtocalLinker : TcpProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        public ModbusRtuInTcpProtocalLinker(string ip)
            : base(ip, int.Parse(ConfigurationManager.AppSettings["ModbusPort"] ?? "502"))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public ModbusRtuInTcpProtocalLinker(string ip, int port)
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
            //ProtocalLinker的CheckRight不会返回null
            if (!base.CheckRight(content).Value) return false;
            //CRC校验失败
            if (!Crc16.GetInstance().CrcEfficacy(content))
                throw new ModbusProtocalErrorException(501);
            //Modbus协议错误
            if (content[1] > 127)
                throw new ModbusProtocalErrorException(content[2]);
            return true;
        }
    }
}