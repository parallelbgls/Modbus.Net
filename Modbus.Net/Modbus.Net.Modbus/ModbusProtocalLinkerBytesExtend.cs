using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Tcp协议字节伸缩
    /// </summary>
    public class ModbusTcpProtocalLinkerBytesExtend : IProtocalLinkerBytesExtend
    {
        public byte[] BytesExtend(byte[] content)
        {
            //Modbus/Tcp协议扩张，前面加6个字节，前面4个为0，后面两个为协议整体内容的长度
            var newFormat = new byte[6 + content.Length];
            var tag = 0;
            var leng = (ushort) content.Length;
            Array.Copy(BigEndianValueHelper.Instance.GetBytes(tag), 0, newFormat, 0, 4);
            Array.Copy(BigEndianValueHelper.Instance.GetBytes(leng), 0, newFormat, 4, 2);
            Array.Copy(content, 0, newFormat, 6, content.Length);
            return newFormat;
        }

        public byte[] BytesDecact(byte[] content)
        {
            //Modbus/Tcp协议收缩，抛弃前面6个字节的内容
            var newContent = new byte[content.Length - 6];
            Array.Copy(content, 6, newContent, 0, newContent.Length);
            return newContent;
        }
    }

    public class ModbusRtuProtocalLinkerBytesExtend : IProtocalLinkerBytesExtend
    {
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

        public byte[] BytesDecact(byte[] content)
        {
            //Modbus/Rtu协议收缩，抛弃后面2个字节的内容
            var newContent = new byte[content.Length - 2];
            Array.Copy(content, 0, newContent, 0, newContent.Length);
            return newContent;
        }
    }

    public class ModbusAsciiProtocalLinkerBytesExtend : IProtocalLinkerBytesExtend
    {
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