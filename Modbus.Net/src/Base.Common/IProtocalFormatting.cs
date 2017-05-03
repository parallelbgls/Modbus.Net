namespace Modbus.Net
{
    /// <summary>
    ///     协议转换的接口
    /// </summary>
    public interface IProtocalFormatting : IProtocalFormatting<byte[], byte[]>
    {
        
    }

    /// <summary>
    ///     协议转换的接口
    /// </summary>
    public interface IProtocalFormatting<out TParamIn, in TParamOut>
    {
        /// <summary>
        ///     从输入结构格式化
        /// </summary>
        /// <param name="message">结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        TParamIn Format(IInputStruct message);

        /// <summary>
        ///     从对象的参数数组格式化
        /// </summary>
        /// <param name="message">非结构化的输入数据</param>
        /// <returns>格式化后的字节流</returns>
        byte[] Format(params object[] message);

        /// <summary>
        ///     把仪器返回的内容填充到输出结构中
        /// </summary>
        /// <param name="messageBytes">返回数据的字节流</param>
        /// <param name="pos">转换标记位</param>
        /// <returns>结构化的输出数据</returns>
        IOutputStruct Unformat(TParamOut messageBytes, ref int pos);

        /// <summary>
		/// 	把仪器返回的内容填充到输出结构中
		/// </summary>
		/// <param name="messageBytes">返回数据的字节流</param>
		/// <param name="pos">转换标记位</param>
		/// <typeparam name="T">IOutputStruct的具体类型</typeparam>
		/// <returns>结构化的输出数据</returns>
        T Unformat<T>(TParamOut messageBytes, ref int pos) where T : class, IOutputStruct;
    }
}