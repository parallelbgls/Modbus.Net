using System.Collections.Generic;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器
    /// </summary>
    public class ModbusRtuProtocolLinker : ComProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusRtuProtocolLinker(string com, int slaveAddress)
            : base(com, slaveAddress)
        {
            ((IConnectorWithController<byte[], byte[]>)BaseConnector).AddController(new FifoController(
                int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")),
                lengthCalc: content =>
                {
                    if (content[1] > 128) return 5;
                    else if (content[1] == 5 || content[1] == 6 || content[1] == 8 || content[1] == 11 || content[1] == 15 || content[1] == 16) return 8;
                    else if (content[1] == 7) return 5;
                    else if (content[1] == 22) return 10;
                    else return DuplicateWithCount.GetDuplcateFunc(new List<int> { 2 }, 5).Invoke(content);
                },
                checkRightFunc: ContentCheck.Crc16CheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) :
                  null
            ));
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