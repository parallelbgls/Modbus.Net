using System;

namespace Modbus.Net
{
    /// <summary>
    ///     协议单元
    /// </summary>
    public abstract class ProtocalUnit : IProtocalFormatting
    {
        /// <summary>
        ///     是否为小端格式
        /// </summary>
        public bool IsLittleEndian { get; protected set; } = false;

        /// <summary>
        ///     从输入结构格式化
        /// </summary>
        /// <param name="message">结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public abstract byte[] Format(InputStruct message);

        /// <summary>
        ///     从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        public virtual byte[] Format(params object[] message)
        {
            return TranslateContent(IsLittleEndian, message);
        }

        /// <summary>
        ///     把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        public abstract OutputStruct Unformat(byte[] messageBytes, ref int pos);

        /// <summary>
        ///     转换静态方法，把对象数组转换为字节数组。
        /// </summary>
        /// <param name="isLittleEndian">是否是小端格式</param>
        /// <param name="contents">对象数组</param>
        /// <returns>字节数组</returns>
        public static byte[] TranslateContent(bool isLittleEndian, params object[] contents)
        {
            return isLittleEndian
                ? ValueHelper.Instance.ObjectArrayToByteArray(contents)
                : BigEndianValueHelper.Instance.ObjectArrayToByteArray(contents);
        }
    }

    /// <summary>
    ///     特殊协议单元，继承这个类的协议不会执行BytesExtend和BytesDecact
    /// </summary>
    public abstract class SpecialProtocalUnit : ProtocalUnit
    {
    }

    /// <summary>
    ///     输入结构
    /// </summary>
    public class InputStruct
    {
    }

    /// <summary>
    ///     输出结构
    /// </summary>
    public class OutputStruct
    {
    }

    /// <summary>
    ///     协议错误
    /// </summary>
    public class ProtocalErrorException : Exception
    {
        public ProtocalErrorException(string message)
            : base(message)
        {
        }
    }
}