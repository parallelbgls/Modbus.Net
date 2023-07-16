using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Udp字节伸缩
    /// </summary>
    public class ModbusRtuInUdpProtocolLinkerBytesExtend : ModbusRtuProtocolLinkerBytesExtend
    {

    }

    /// <summary>
    ///     Udp字节伸缩
    /// </summary>
    public class ModbusAsciiInUdpProtocolLinkerBytesExtend : ModbusAsciiProtocolLinkerBytesExtend
    {

    }

    /// <summary>
    ///     Udp字节伸缩
    /// </summary>
    public class ModbusUdpProtocolLinkerBytesExtend : ModbusTcpProtocolLinkerBytesExtend
    {

    }

    /// <summary>
    ///     Rtu透传字节伸缩
    /// </summary>
    public class ModbusRtuInTcpProtocolLinkerBytesExtend : ModbusRtuProtocolLinkerBytesExtend
    {

    }

    /// <summary>
    ///     Ascii透传字节伸缩
    /// </summary>
    public class ModbusAsciiInTcpProtocolLinkerBytesExtend : ModbusAsciiProtocolLinkerBytesExtend
    {

    }

    /// <summary>
    ///     Tcp协议字节伸缩
    /// </summary>
    public class ModbusTcpProtocolLinkerBytesExtend : IProtocolLinkerBytesExtend<byte[], byte[]>
    {
        private static ushort _sendCount = 0;
        private static readonly object _counterLock = new object();

        /// <summary>
        ///     协议扩展，协议内容发送前调用
        /// </summary>
        /// <param name="content">扩展前的原始协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            //Modbus/Tcp协议扩张，前面加6个字节，前面2个为事务编号，中间两个为0，后面两个为协议整体内容的长度
            var newFormat = new byte[6 + content.Length];
            lock (_counterLock)
            {
                var transaction = (ushort)(_sendCount % 65536 + 1);
                var tag = (ushort)0;
                var leng = (ushort)content.Length;
                Array.Copy(BigEndianLsbValueHelper.Instance.GetBytes(transaction), 0, newFormat, 0, 2);
                Array.Copy(BigEndianLsbValueHelper.Instance.GetBytes(tag), 0, newFormat, 2, 2);
                Array.Copy(BigEndianLsbValueHelper.Instance.GetBytes(leng), 0, newFormat, 4, 2);
                Array.Copy(content, 0, newFormat, 6, content.Length);
                _sendCount++;
            }
            return newFormat;
        }

        /// <summary>
        ///     协议收缩，协议内容接收后调用
        /// </summary>
        /// <param name="content">收缩前的完整协议内容</param>
        /// <returns>收缩后的协议内容</returns>
        public byte[] BytesDecact(byte[] content)
        {
            //Modbus/Tcp协议收缩，抛弃前面6个字节的内容
            var newContent = new byte[content.Length - 6];
            Array.Copy(content, 6, newContent, 0, newContent.Length);
            return newContent;
        }
    }

    /// <summary>
    ///     Rtu协议字节伸缩
    /// </summary>
    public class ModbusRtuProtocolLinkerBytesExtend : IProtocolLinkerBytesExtend<byte[], byte[]>
    {
        /// <summary>
        ///     协议扩展，协议内容发送前调用
        /// </summary>
        /// <param name="content">扩展前的原始协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            var crc = new byte[2];
            //Modbus/Rtu协议扩张，增加CRC校验
            var newFormat = new byte[content.Length + 2];
            Crc16.GetInstance().GetCRC(content, ref crc);
            Array.Copy(content, 0, newFormat, 0, content.Length);
            Array.Copy(crc, 0, newFormat, newFormat.Length - 2, crc.Length);
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

    /// <summary>
    ///     Ascii协议字节伸缩
    /// </summary>
    public class ModbusAsciiProtocolLinkerBytesExtend : IProtocolLinkerBytesExtend<byte[], byte[]>
    {
        /// <summary>
        ///     协议扩展，协议内容发送前调用
        /// </summary>
        /// <param name="content">扩展前的原始协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public byte[] BytesExtend(byte[] content)
        {
            //Modbus/Ascii协议扩张，前面增加:，后面增加LRC校验和尾字符
            var newContent = new List<byte>();
            newContent.AddRange(Encoding.ASCII.GetBytes(":"));
            foreach (var number in content)
                newContent.AddRange(Encoding.ASCII.GetBytes(number.ToString("X2")));
            newContent.AddRange(Encoding.ASCII.GetBytes(Crc16.GetInstance().GetLRC(content)));
            newContent.Add(0x0d);
            newContent.Add(0x0a);
            return newContent.ToArray();
        }

        /// <summary>
        ///     协议收缩，协议内容接收后调用
        /// </summary>
        /// <param name="content">收缩前的完整协议内容</param>
        /// <returns>收缩后的协议内容</returns>
        public byte[] BytesDecact(byte[] content)
        {
            //Modbus/Ascii协议收缩，抛弃头尾。
            var newContent = new List<byte>();
            var ans = Encoding.ASCII.GetString(content);
            var index = ans.IndexOf(Environment.NewLine);
            ans = ans.Substring(1, index - 1);
            for (var i = 0; i < ans.Length; i += 2)
            {
                var number = byte.Parse(ans.Substring(i, 2), NumberStyles.HexNumber);
                newContent.Add(number);
            }
            newContent.RemoveAt(newContent.Count - 1);
            return newContent.ToArray();
        }
    }
}