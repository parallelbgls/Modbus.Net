using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus长度计算
    /// </summary>
    public static class ModbusLengthCalc
    {
        /// <summary>
        ///     Modbus Ascii协议长度计算
        /// </summary>
        public static Func<byte[], int> ModbusAsciiLengthCalc => content =>
        {
            if (content[0] != 0x3a) return 0;
            for (int i = 1; i < content.Length; i++)
            {
                if (content[i - 1] == 0x0D && content[i] == 0x0A) return i + 1;
            }
            return -1;
        };

        /// <summary>
        ///     Modbus Rtu协议长度计算
        /// </summary>
        public static Func<byte[], int> ModbusRtuLengthCalc => content =>
        {
            if (content[1] > 128) return 5;
            else if (content[1] == 5 || content[1] == 6 || content[1] == 8 || content[1] == 11 || content[1] == 15 || content[1] == 16) return 8;
            else if (content[1] == 7) return 5;
            else if (content[1] == 22) return 10;
            else return DuplicateWithCount.GetDuplcateFunc(new List<int> { 2 }, 5).Invoke(content);
        };
    }

    /// <summary>
    ///     Modbus Ascii协议控制器
    /// </summary>
    public class ModbusAsciiController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusAsciiController(string com, int slaveAddress) : base(
                int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusAsciiLengthCalc,
                checkRightFunc: ContentCheck.LrcCheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) :
                  null
             )
        { }
    }

    /// <summary>
    ///     Modbus Ascii in Tcp协议控制器
    /// </summary>
    public class ModbusAsciiInTcpController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusAsciiInTcpController(string ip, int port) : base(int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusAsciiLengthCalc,
                checkRightFunc: ContentCheck.LrcCheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null
                  ? int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount"))
                  : null
             )
        { }
    }

    /// <summary>
    ///     Modbus Ascii in Udp协议控制器
    /// </summary>
    public class ModbusAsciiInUdpController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusAsciiInUdpController(string ip, int port) : base(int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusAsciiLengthCalc,
                checkRightFunc: ContentCheck.LrcCheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount") != null
                  ? int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount"))
                  : null
             )
        { }
    }

    /// <summary>
    ///     Modbus Rtu协议控制器
    /// </summary>
    public class ModbusRtuController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusRtuController(string com, int slaveAddress) : base(
                int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusRtuLengthCalc,
                checkRightFunc: ContentCheck.Crc16CheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("COM:" + com + ":" + slaveAddress, "WaitingListCount")) :
                  null
            )
        { }
    }

    /// <summary>
    ///     Modbus Rtu in Tcp协议控制器
    /// </summary>
    public class ModbusRtuInTcpController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusRtuInTcpController(string ip, int port) : base(
                int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusRtuLengthCalc,
                checkRightFunc: ContentCheck.Crc16CheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount")) :
                  null
             )
        { }
    }

    /// <summary>
    ///     Modbus Rtu in Udp协议控制器
    /// </summary>
    public class ModbusRtuInUdpController : FifoController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusRtuInUdpController(string ip, int port) : base(
                int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "FetchSleepTime")),
                lengthCalc: ModbusLengthCalc.ModbusRtuLengthCalc,
                checkRightFunc: ContentCheck.Crc16CheckRight,
                waitingListMaxCount: ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount")) :
                  null
            )
        { }
    }

    /// <summary>
    ///     Modbus Tcp协议控制器
    /// </summary>
    public class ModbusTcpController : ModbusEthMatchDirectlySendController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusTcpController(string ip, int port) : base(
                new ICollection<(int, int)>[] { new List<(int, int)> { (0, 0), (1, 1) } },
                lengthCalc: DuplicateWithCount.GetDuplcateFunc(new List<int> { 4, 5 }, 6),
                waitingListMaxCount: ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("TCP:" + ip + ":" + port, "WaitingListCount")) :
                  null
            )
        { }
    }

    /// <summary>
    ///     Modbus Udp协议控制器
    /// </summary>
    public class ModbusUdpController : ModbusEthMatchDirectlySendController
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        public ModbusUdpController(string ip, int port) : base(
                new ICollection<(int, int)>[] { new List<(int, int)> { (0, 0), (1, 1) } },
                lengthCalc: DuplicateWithCount.GetDuplcateFunc(new List<int> { 4, 5 }, 6),
                waitingListMaxCount: ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount") != null ?
                  int.Parse(ConfigurationReader.GetValue("UDP:" + ip + ":" + port, "WaitingListCount")) :
                  null
            )
        { }
    }

    /// <summary>
    ///     匹配控制器，载入队列后直接发送
    /// </summary>
    public class ModbusEthMatchDirectlySendController : MatchDirectlySendController
    {
        /// <inheritdoc />
        public ModbusEthMatchDirectlySendController(ICollection<(int, int)>[] keyMatches,
            Func<byte[], int> lengthCalc = null, Func<byte[], bool?> checkRightFunc = null, int? waitingListMaxCount = null) : base(keyMatches,
            lengthCalc, checkRightFunc, waitingListMaxCount)
        {
        }

        /// <inheritdoc />
        protected override MessageWaitingDef GetMessageFromWaitingList(byte[] receiveMessage)
        {
            MessageWaitingDef ans;
            if (receiveMessage[0] == 0 && receiveMessage[1] == 0)
            {
                lock (WaitingMessages)
                {
                    ans = WaitingMessages.FirstOrDefault();
                }
            }
            else
            {
                var returnKey = GetKeyFromMessage(receiveMessage);
                lock (WaitingMessages)
                {
                    ans = WaitingMessages.FirstOrDefault(p => returnKey.HasValue && p.Key == returnKey.Value.Item2);
                }
            }
            return ans;
        }
    }
}


