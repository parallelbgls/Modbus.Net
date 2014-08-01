using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    /// <summary>
    /// 协议字节伸缩
    /// </summary>
    public abstract class ProtocalLinkerBytesExtend
    {
        /// <summary>
        /// 协议扩展，协议内容发送前调用
        /// </summary>
        /// <param name="content">扩展前的原始协议内容</param>
        /// <returns>扩展后的协议内容</returns>
        public abstract byte[] BytesExtend(byte[] content);

        /// <summary>
        /// 协议收缩，协议内容接收后调用
        /// </summary>
        /// <param name="content">收缩前的完整协议内容</param>
        /// <returns>收缩后的协议内容</returns>
        public abstract byte[] BytesDecact(byte[] content);
    }
    
    /// <summary>
    /// Tcp协议字节伸缩
    /// </summary>
    public class ModbusTcpProtocalLinkerBytesExtend : ProtocalLinkerBytesExtend
    {
        public override byte[] BytesExtend(byte[] content)
        {
            //Modbus/Tcp协议扩张，前面加6个字节，前面4个为0，后面两个为协议整体内容的长度
            byte[] newFormat = new byte[6 + content.Length];
            int tag = 0;
            ushort leng = (ushort)content.Length;
            Array.Copy(ValueHelper.Instance.GetBytes(tag), 0, newFormat, 0, 4);
            Array.Copy(ValueHelper.Instance.GetBytes(leng), 0, newFormat, 4, 2);
            Array.Copy(content, 0, newFormat, 6, content.Length);
            return newFormat;
        }

        public override byte[] BytesDecact(byte[] content)
        {
            //Modbus/Tcp协议收缩，抛弃前面6个字节的内容
            byte[] newContent = new byte[content.Length - 6];
            Array.Copy(content, 6, newContent, 0, newContent.Length);
            return newContent;
        }
    }
}
