using System;

namespace ModBus.Net
{
    public abstract class ProtocalUnit : IProtocalFormatting
    {
        /// <summary>
        /// 格式化，将输入结构转换为字节数组
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract byte[] Format(InputStruct message);

        /// <summary>
        /// 格式化，将对象数组转换为字节数组
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual byte[] Format(params object[] message)
        {
            return TranslateContent(message);
        }

        /// <summary>
        /// 结构化，将字节数组转换为输出结构。
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract OutputStruct Unformat(byte[] messageBytes, ref int pos);

        /// <summary>
        /// 转换静态方法，把对象数组转换为字节数组。
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static byte[] TranslateContent(params object[] contents)
        {
            return ValueHelper.Instance.ObjectArrayToByteArray(contents);
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