using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.HJ212
{
    /// <summary>
    ///     HJ212协议
    /// </summary>
    public class HJ212Protocol : BaseProtocol
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        public HJ212Protocol(string ip)
            : base(0, 0, Endian.BigEndianLsb)
        {
            ProtocolLinker = new HJ212ProtocolLinker(ip, int.Parse(ConfigurationReader.GetValueDirect("TCP:" + ip, "HJ212Port") ?? ConfigurationReader.GetValueDirect("TCP:Modbus", "HJ212Port") ?? "443"));
        }

        /// <summary>
        ///     连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            return await ProtocolLinker.ConnectAsync();
        }
    }

    #region 写数据
    /// <summary>
    ///     写数据协议
    /// </summary>
    public class WriteRequestHJ212Protocol : ProtocolUnit<byte[], byte[]>
    {
        /// <summary>
        ///     从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public override byte[] Format(IInputStruct message)
        {
            var r_message = (WriteRequestHJ212InputStruct)message;
            string formatMessage = "##0633";
            formatMessage += "QN=" + r_message.QN + ";";
            formatMessage += "ST=" + r_message.ST + ";";
            formatMessage += "CN=" + r_message.CN + ";";
            formatMessage += "PW=" + r_message.PW + ";";
            formatMessage += "MN=" + r_message.MN + ";";
            formatMessage += "CP=&&";
            formatMessage += "DateTime=" + r_message.Datetime.ToString("yyyyMMddHHmmss") + ";";
            foreach (var record in r_message.CP)
            {
                foreach (var data in record)
                {
                    formatMessage += data.Key + "=" + data.Value + ",";
                }
                formatMessage = formatMessage[..^1];
                formatMessage += ";";
            }
            formatMessage = formatMessage[..^1];
            formatMessage += "&&";
            return Encoding.ASCII.GetBytes(formatMessage);
        }

        /// <summary>
        ///     把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        public override IOutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            return new WriteRequestHJ212OutputStruct(Encoding.ASCII.GetString(messageBytes));
        }
    }

    /// <summary>
    ///     写数据输入
    /// </summary>
    public class WriteRequestHJ212InputStruct : IInputStruct
    {
        public WriteRequestHJ212InputStruct(string st, string cn, string pw, string mn, List<Dictionary<string, string>> cp, DateTime datetime)
        {
            ST = st;
            CN = cn;
            PW = pw;
            MN = mn;
            CP = cp;
            Datetime = datetime;
        }

        public string QN => "20170101000926706";

        public string ST { get; }

        public string CN { get; }

        public string PW { get; }

        public string MN { get; }

        public List<Dictionary<string, string>> CP { get; }

        public DateTime Datetime { get; }
    }

    /// <summary>
    ///     写数据输出
    /// </summary>
    public class WriteRequestHJ212OutputStruct : IOutputStruct
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="value">读取的数据</param>
        public WriteRequestHJ212OutputStruct(string value)
        {
            GetValue = value;
        }

        /// <summary>
        ///     读取的地址
        /// </summary>
        public string GetValue { get; private set; }
    }
    #endregion
}