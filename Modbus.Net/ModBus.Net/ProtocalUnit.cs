using System;

namespace ModBus.Net
{
    public abstract class ProtocalUnit : IProtocalFormatting
    {
        /// <summary>
        /// 从输入结构格式化
        /// </summary>
        /// <param name="message">结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public abstract byte[] Format(InputStruct message);

        /// <summary>
        /// 从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public virtual byte[] Format(params object[] message)
        {
            return TranslateContent(message);
        }

        /// <summary>
        /// 把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        public abstract OutputStruct Unformat(byte[] messageBytes, ref int pos);

        /// <summary>
        /// 转换静态方法，把对象数组转换为字节数组。
        /// </summary>
        /// <param name="contents">对象数组</param>
        /// <returns>字节数组</returns>
        public static byte[] TranslateContent(params object[] contents)
        {
            return BigEndianValueHelper.Instance.ObjectArrayToByteArray(contents);
        }
    }

    public abstract class SpecialProtocalUnit : ProtocalUnit
    {
        
    }

    /// <summary>
    /// 输入结构
    /// </summary>
    public class InputStruct
    {
    }

    /// <summary>
    /// 输出结构
    /// </summary>
    public class OutputStruct
    {
    }

    /// <summary>
    /// 协议错误
    /// </summary>
    public class ProtocalErrorException : Exception
    {
        public ProtocalErrorException(string message)
            : base(message)
        {

        }
    }
}