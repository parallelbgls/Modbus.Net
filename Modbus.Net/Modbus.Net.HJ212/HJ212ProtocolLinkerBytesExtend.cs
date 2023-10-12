using System;
using System.Text;

namespace Modbus.Net.HJ212
{
    /// <summary>
    ///     Rtu协议字节伸缩
    /// </summary>
    public class HJ212ProtocolLinkerBytesExtend : IProtocolLinkerBytesExtend<byte[], byte[]>
    {
        /// <summary>
        ///     协议扩展，协议内容发送前调用
        /// </summary>
        /// <param name="content">扩展前的原始协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            var newFormat = new byte[content.Length + 12];
            Array.Copy(content, 0, newFormat, 6, content.Length);
            //表头长度扩张
            var length = content.Length;
            string lengthString = length.ToString("0000");
            lengthString = "##" + lengthString;
            var lengthCalc = Encoding.ASCII.GetBytes(lengthString);
            Array.Copy(lengthCalc, 0, newFormat, 0, 6);
            //Modbus/Rtu协议扩张，增加CRC校验
            var crc = new byte[2];
            Crc16.GetInstance().GetCRC(content, ref crc);
            string crcString = BitConverter.ToString(crc).Replace("-", string.Empty);
            crcString = "&&" + crcString;
            var crcCalc = Encoding.ASCII.GetBytes(crcString);
            Array.Copy(crcCalc, 0, newFormat, newFormat.Length - 6, 6);
            return newFormat;
        }

        /// <summary>
        ///     协议收缩，协议内容接收后调用
        /// </summary>
        /// <param name="content">收缩前的完整协议内容</param>
        /// <returns>收缩后的协议内容</returns>
        public byte[] BytesDecact(byte[] content)
        {
            //Modbus/Rtu协议收缩，抛弃后面2个字节的内容
            var newContent = new byte[content.Length - 2];
            Array.Copy(content, 0, newContent, 0, newContent.Length);
            return newContent;
        }
    }
}
