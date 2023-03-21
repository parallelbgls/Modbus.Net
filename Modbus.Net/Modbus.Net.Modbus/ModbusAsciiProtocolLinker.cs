using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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
            : base(com, slaveAddress)
        {
            ((BaseConnector)BaseConnector).AddController(new MatchController(new ICollection<(int, int)>[] { new List<(int, int)> { (1, 1), (2, 2) }, new List<(int, int)> { (3, 3), (4, 4) } }, int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")), waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ? int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) : null));
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