using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.HJ212
{
    /// <summary>
    ///     HJ212协议连接器
    /// </summary>
    public class HJ212ProtocolLinker : TcpProtocolLinker
    {
        public HJ212ProtocolLinker(string ip, int port) : base(ip, port)
        {
        }

        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            if (content.Length <= 1000)
                return await base.SendReceiveAsync(content);
            else
            {
                string contentString = Encoding.ASCII.GetString(content);
                string[] formats = { "yyyyMMddHHmmssffff", "yyyyMMddHHmmssfff", "yyyyMMddHHmmss" };
                DateTime startTime = DateTime.ParseExact(contentString.Substring(3, 18), formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                int dataStartIdx = contentString.IndexOf("&&") + 2;
                string head = contentString.Substring(0, dataStartIdx).Substring(21);
                string[] contentUnitAll = contentString.Substring(dataStartIdx).Split(';')[1].Split(',');
                List<string> contentSplitAll = new List<string>();
                string newContent = "DataTime=" + startTime.ToString("yyyyMMddHHmmss") + ";";
                foreach (var contentUnit in contentUnitAll)
                {
                    if (newContent.Length + contentUnit.Length > 1000 - dataStartIdx)
                    {
                        contentSplitAll.Add("QN=" + startTime.ToString("yyyyMMddHHmmssffff") + head + newContent[..^1]);
                        startTime = startTime.AddSeconds(1);
                        newContent = "DataTime=" + startTime.ToString("yyyyMMddHHmmss") + ";" + contentUnit + ",";
                    }
                    else
                    {
                        newContent += contentUnit + ",";
                    }
                }
                contentSplitAll.Add("QN=" + startTime.ToString("yyyyMMddHHmmssffff") + head + newContent[..^1]);
                var receiveBytesAll = new List<byte>();
                foreach (var contentSplit in contentSplitAll)
                {
                    var extBytes = BytesExtend(Encoding.ASCII.GetBytes(contentSplit));
                    var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
                    receiveBytesAll.AddRange(receiveBytes == null ? null : receiveBytes.Length == 0 ? receiveBytes : BytesDecact(receiveBytes));
                }
                return receiveBytesAll.ToArray();
            }
        }

        /// <summary>
        ///     检查接收的数据是否正确
        /// </summary>
        /// <param name="content">接收协议的内容</param>
        /// <returns>协议是否是正确的</returns>
        public override bool? CheckRight(byte[] content)
        {
            return true;
        }
    }
}
