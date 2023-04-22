using System.Collections.Generic;
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
            ((IConnectorWithController<byte[], byte[]>)BaseConnector).AddController(new FifoController(
                int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")),
                lengthCalc: content =>
                {
                    if (content[0] != 0x3a) return 0;
                    for (int i = 1; i < content.Length; i++)
                    {
                        if (content[i - 1] == 0x0D && content[i] == 0x0A) return i + 1;
                    }
                    return -1;
                },
                checkRightFunc: ContentCheck.LrcCheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) :
                  null
             ));
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