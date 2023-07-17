using System.Collections.Generic;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Ppi协议控制器
    /// </summary>
    public class SiemensPpiController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口</param>
        /// <param name="slaveAddress">从站号</param>
        public SiemensPpiController(string com, int slaveAddress) : base(
                int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")),
                lengthCalc: content =>
                {
                    if (content[0] == 0x10)
                        return 6;
                    else if (content[0] == 0xE5)
                        return 1;
                    else
                        return DuplicateWithCount.GetDuplcateFunc(new List<int> { 1 }, 6)(content);
                },
                checkRightFunc: ContentCheck.FcsCheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) :
                  null
             )
        { }
    }

    /// <summary>
    ///     西门子Tcp协议控制器
    /// </summary>
    public class SiemensTcpController : MatchDirectlySendController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public SiemensTcpController(string ip, int port) : base(
                new ICollection<(int, int)>[] { new List<(int, int)> { (11, 11), (12, 12) } },
                lengthCalc: DuplicateWithCount.GetDuplcateFunc(new List<int> { 2, 3 }, 0),
                waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount")) :
                  null
             )
        { }
    }
}
