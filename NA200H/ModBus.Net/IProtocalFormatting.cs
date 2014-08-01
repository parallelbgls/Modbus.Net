namespace ModBus.Net
{
    /// <summary>
    /// 协议转换的接口
    /// </summary>
    public interface IProtocalFormatting
    {
        /// <summary>
        /// 从输入结构格式化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Format(InputStruct message);

        /// <summary>
        /// 从对象的参数数组格式化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Format(params object[] message);

        /// <summary>
        /// 把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        OutputStruct Unformat(byte[] messageBytes, ref int pos);
    }
}