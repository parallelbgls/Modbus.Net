using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Ascii码协议连接器
    /// </summary>
    public class ModbusAsciiProtocolLinker : ComProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusAsciiProtocolLinker(string com, int slaveAddress)
            : base(com, 9600, Parity.None, StopBits.One, 8, slaveAddress)
        {
            ((BaseConnector)BaseConnector).AddController(new MatchController(new ICollection<int>[] { new List<int> { 0, 1 }}, 500));
        }

        /// <summary>
        ///     校验返回数据是否正确
        /// </summary>
        /// <param name="content">返回的数据</param>
        /// <returns>校验是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocolLinker不会返回null
            if (!base.CheckRight(content).Value) return false;
            //CRC校验失败
            var contentString = Encoding.ASCII.GetString(content);
            if (!Crc16.GetInstance().LrcEfficacy(contentString))
                throw new ModbusProtocolErrorException(501);
            //Modbus协议错误
            if (byte.Parse(contentString.Substring(3, 2)) > 127)
                throw new ModbusProtocolErrorException(byte.Parse(contentString.Substring(5, 2)));
            return true;
        }
    }
}